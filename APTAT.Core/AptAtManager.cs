using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using APTAT.Core.Apt;
using APTAT.Core.Descent;
using APTAT.Core.Facade.Descent.Commands;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using APTAT.Core.Facade.Descent.MessageContracts;
using APTAT.Core.Reports;
using APTAT.Core.Valkyrie;
using APTAT.Valkyrie.Client;
using Valkyrie.WebApiProvider.Contracts;
using Valkyrie.WebApiProvider.Enums;

namespace APTAT.Core
{
    public enum ETargetsDiscovererType
    {
        ActiveDirectory,
        NetworkAddresses,
        Workgroup
    }

    public class DevicesSourceDescription
    {
        public ETargetsDiscovererType Type { get; set; }
        public object Description { get; set; }

        public static string GetNetworkDescription(string[] ipAddressesStringArray)
        {
            if (ipAddressesStringArray == null)
                throw new InvalidOperationException("Could not get Network Description");

            int maxElementsInResultString = 3;

            var resultAddrAray = ipAddressesStringArray.Take(maxElementsInResultString).ToArray();
            string resultAddrString = string.Join("; ", resultAddrAray);

            if (ipAddressesStringArray.Length > maxElementsInResultString)
                resultAddrString += " ...";

            return resultAddrString;
        }
    }

    public interface ITargetsDiscoverer
    {
        DeploymentTargetsCollection Discover();
        void BeginDiscover(object state = null);
        event EventHandler<TargetsDiscovererEventArgs> Discovered;
    }

    public class TargetsDiscovererEventArgs : EventArgs
    {
        public Exception Error { get; set; }
        public DeploymentTarget[] Targets { get; set; }
        public object State { get; set; }
    }

    internal abstract class TargetsDiscoverer : ITargetsDiscoverer
    {
        private IServiceCommand<FindTargetsResponse> _command;
        public event EventHandler<TargetsDiscovererEventArgs> Discovered;

        internal TargetsDiscoverer()
        {
        }

        internal TargetsDiscoverer(IServiceCommand<FindTargetsResponse> command)
        {
            _command = command;
        }

        public DeploymentTargetsCollection Discover()
        {
            if (_command != null)
            {
                _command.Execute();
                return _command.Response.Targets;
            }

            return null;
        }

        public void BeginDiscover(object state = null)
        {
            if (_command != null)
            {
                Action action = _command.Execute;
                Context context = new Context() { Action = action, State = state };
                action.BeginInvoke(BeginDiscoverCallback, context);
            }
        }

        private void BeginDiscoverCallback(IAsyncResult ar)
        {
            Context context = ar.AsyncState as Context;
            if (context == null)
                return;

            TargetsDiscovererEventArgs args = new TargetsDiscovererEventArgs();
            try
            {
                context.Action.EndInvoke(ar);
                args.Targets = _command.Response.Targets.ToArray();
                args.State = context.State;
            }
            catch (DirectoryServicesCOMException ex)
            {
                LogHelper.Trace(TraceEventType.Error, "AptManager BeginDiscoverCallback DirectoryServicesCOMException: {0}.", ex.Message);
                args.Error = new Exception(String.Format("An error occured while discovering Active Directory: {0}", ex.Message));
                args.State = context.State;
            }
            catch (Exception ex)
            {
                LogHelper.Trace(TraceEventType.Error, "AptManager BeginDiscoverCallback error: {0}", ex.Message);
                args.Error = ex;
                args.State = context.State;
            }
            finally
            {
                OnDiscovered(args);
            }
        }

        protected virtual void OnDiscovered(TargetsDiscovererEventArgs args)
        {
            EventHandler<TargetsDiscovererEventArgs> temp = Discovered;
            if (temp != null)
                temp(this, args);
        }

        public IServiceCommand<FindTargetsResponse> Command
        {
            get { return _command; }
            set { _command = value; }
        }

        internal class Context
        {
            internal Action Action { get; set; }
            internal object State { get; set; }
        }
    }

    public interface IDomainDiscoverer : ITargetsDiscoverer
    {
    }

    internal class DomainDiscoverer : TargetsDiscoverer, IDomainDiscoverer
    {
        internal DomainDiscoverer()
        {
        }

        internal DomainDiscoverer(IServiceCommand<FindTargetsResponse> command)
            : base(command)
        {
        }
    }

    public interface IWorkgroupDiscoverer : ITargetsDiscoverer
    {
    }

    internal class WorkgroupDiscoverer : TargetsDiscoverer, IWorkgroupDiscoverer
    {
        internal WorkgroupDiscoverer()
        {
        }

        internal WorkgroupDiscoverer(IServiceCommand<FindTargetsResponse> command)
            : base(command)
        {
        }
    }

    public interface IIpAddressDiscoverer : ITargetsDiscoverer
    {
    }

    internal class IpAddressDiscoverer : TargetsDiscoverer, IIpAddressDiscoverer
    {
        internal IpAddressDiscoverer()
        {
        }

        internal IpAddressDiscoverer(IServiceCommand<FindTargetsResponse> command)
            : base(command)
        {
        }
    }


    public interface ITargetsResolver
    {
        DeploymentTargetsCollection Resolve();
        void BeginResolve(object state = null);
        event EventHandler<TargetsResolverEventArgs> TargetsResolved;
    }

    public interface IDomainTargetsResolver : ITargetsResolver
    {
    }

    internal class DomainTargetsResolver : IDomainTargetsResolver
    {
        private IServiceCommand<GetTargetsSummaryResponse> _command;
        public event EventHandler<TargetsResolverEventArgs> TargetsResolved;

        public DomainTargetsResolver()
        {
        }

        public DomainTargetsResolver(IServiceCommand<GetTargetsSummaryResponse> command)
        {
            _command = command;
        }

        public DeploymentTargetsCollection Resolve()
        {
            if (_command != null)
            {
                _command.Execute();
                return _command.Response.Targets;
            }

            return null;
        }

        public void BeginResolve(object state = null)
        {
            if (_command != null)
            {
                Action action = _command.Execute;
                Context context = new Context() { Action = action, State = state };
                action.BeginInvoke(BeginResolveCallback, context);
            }
        }

        private void BeginResolveCallback(IAsyncResult ar)
        {
            Context context = ar.AsyncState as Context;
            if (context == null)
                return;

            TargetsResolverEventArgs args = new TargetsResolverEventArgs();
            try
            {
                context.Action.EndInvoke(ar);
                args.Targets = _command.Response.Targets.ToArray();
                args.State = context.State;
            }
            catch (DirectoryServicesCOMException ex)
            {
                LogHelper.Trace(TraceEventType.Error, "AptManager BeginDiscoverCallback DirectoryServicesCOMException: {0}.", ex.Message);
                args.Error = new Exception(String.Format("An error occured while discovering Active Directory: {0}", ex.Message));
                args.State = context.State;
            }
            catch (Exception ex)
            {
                LogHelper.Trace(TraceEventType.Error, "AptManager BeginDiscoverCallback error: {0}", ex.Message);
                args.Error = ex;
                args.State = context.State;
            }
            finally
            {
                OnTargetsResolved(args);
            }
        }

        protected virtual void OnTargetsResolved(TargetsResolverEventArgs args)
        {
            EventHandler<TargetsResolverEventArgs> temp = TargetsResolved;
            if (temp != null)
                temp(this, args);
        }

        public IServiceCommand<GetTargetsSummaryResponse> Command
        {
            get { return _command; }
            set { _command = value; }
        }

        internal class Context
        {
            internal Action Action { get; set; }
            internal object State { get; set; }
        }
    }

    public class TargetsResolverEventArgs : EventArgs
    {
        public Exception Error { get; set; }
        public DeploymentTarget[] Targets { get; set; }
        public object State { get; set; }
    }


    public interface IAptScanner
    {
        void BeginScan(object state = null);
        event EventHandler<ScanStartedEventArgs> ScanStarted;
        event EventHandler<ScanCompletedEventArgs> ScanCompleted;
        event EventHandler<ScanInProgressEventArgs> ScanInProgress;
    }

    public class ScanStartedEventArgs : EventArgs
    {
        public Exception Error { get; set; }
        public Guid OperationUid { get; set; }
        public object State { get; set; }
    }

    public class ScanCompletedEventArgs : EventArgs
    {
        public Guid OperationUid { get; set; }
        public int TotalCount { get; set; }
        public int NotPerformedCount { get; set; }
        public int SucceededCount { get; set; }
        public int FailedCount { get; set; }
        public int CalceledCount { get; set; }

        public FilesStatistics FilesStatistics { get; set; }
    }

    public class ScanInProgressEventArgs : EventArgs
    {
        public Guid OperationUid { get; set; }
        public int Percentage { get; set; }
    }



    public class AptAtManager
    {
        private readonly CommandFactory _commandFactory;
        private readonly IDescentSubSystem _descentSubSystem;
        public event ProgressChangedEventHandler ValkyrieAnalyzeProgressChanged;
        public event AnalyzeCompletedEventHandler<FileResult> ValkyrieAnalyzeCompleted;
        private ValkyrieClient _valkyrieClient;        
        public AptAtManager()
            : this(null)
        {
        }

        

        public AptAtManager(IChangeTrackingListener listener)
        {
            var catalog = new SubSystemCatalog();

            _descentSubSystem = new DescentSubSystem();
            _descentSubSystem.Init(catalog);
            _commandFactory = new CommandFactory(_descentSubSystem);
            var changeTrackingSubSystem = catalog.Find<IChangeTrackingSubSystem>();
           
            _valkyrieClient = new ValkyrieClient();
            _valkyrieClient.AnalyzeProgressChanged += OnValkyrieClientProgressChanged;
            _valkyrieClient.AnalyzeCompleted += OnValkyrieManagerAnalyzeCompleted;
            if (listener != null)
                changeTrackingSubSystem.RegisterListener(listener);
        }

        void OnValkyrieManagerAnalyzeCompleted(object sender, AnalyzeCompletedEventArgs<ValkyrieAnalysisResult> e)
        {
            var results = _descentSubSystem.GetFirstScanResults().ScanResults;
            var valkyrieAnalysisResult = GetFullResult(e.Result, results);
            var handler = ValkyrieAnalyzeCompleted;
            ValkyrieAnalysisResult = valkyrieAnalysisResult;
            if (handler != null)
            {
                var arg = new AnalyzeCompletedEventArgs<FileResult>(valkyrieAnalysisResult.ToArray(), e.Error, e.Cancelled, e.UserState);
                handler(this, arg);
            }
        }

        void OnValkyrieClientProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            var handler = ValkyrieAnalyzeProgressChanged;
            if (handler != null)
            {
                ProgressInfo<ValkyrieAnalysisResult> info = (ProgressInfo<ValkyrieAnalysisResult>)e.UserState;
                
                var results = _descentSubSystem.GetFirstScanResults().ScanResults;
                var valkyrieAnalysisResult = GetFullResult(info.Results, results);
                var malwareFilesCount =
                        valkyrieAnalysisResult.Where(i => i.ValkyrieAnalysisResult != null)
                            .Count(i => i.ValkyrieAnalysisResult.Verdict == InfoVerdictEnum.Malware ||
                                i.ValkyrieAnalysisResult.Verdict == InfoVerdictEnum.PotentiallyUnwantedApplication);


                var unknownFilesCount = valkyrieAnalysisResult.Count - malwareFilesCount;

                ProgressInfo<FileResult> progressInfo = new ProgressInfo<FileResult>(info.TotalFileCount, info.ProcessedFileCount, malwareFilesCount, unknownFilesCount, valkyrieAnalysisResult.ToArray());
                handler(this, new ProgressChangedEventArgs(e.ProgressPercentage, progressInfo));
            }
        }

        private static List<FileResult> GetFullResult(IEnumerable<ValkyrieAnalysisResult> valkyrieResult, ConcurrentDictionary<Guid, ScanResult> scanResults)
        {
            List<FileResult> fileResults = new List<FileResult>();

            foreach (var scanResult in scanResults)
            {
                foreach (var badFile in scanResult.Value.BadFiles)
                {
                    var found = fileResults.SingleOrDefault(f => f.Sha1 == badFile.Sha1);

                    if (found == null)
                    {
                        found = new FileResult(badFile);
                        var vResult = valkyrieResult.Where(v => v.SHA1 != null).Where(v => v.SHA1.ToUpper() == badFile.Sha1.ToUpper()).FirstOrDefault();

                        if (vResult != null)
                        {
                            found.ValkyrieAnalysisResult = vResult;
                            fileResults.Add(found);
                        }
                    }
                  
                    if (scanResult.Value.AptReport != null && scanResult.Value.AptReport.Computer != null)
                    {
                        var computer = scanResult.Value.AptReport.Computer;
                        var comp = new ComputerInformation(computer);
                        if (!found.Computers.Any(c => c.IP == comp.IP))
                        {                        
                            comp.FilePath = badFile.Path;
                            found.Computers.Add(comp);
                        }
                    }
                }
            }
            return fileResults;
        }

        public static List<ValkyrieFileInfo> GetDistinctFileInfo(ConcurrentDictionary<Guid, ScanResult> results)
        {
            Dictionary<string, ScanResult> scanResults = results.Select(r => r.Value).ToDictionary(t => t.TargetUid.ToString());

            List<ValkyrieFileInfo> files = new List<ValkyrieFileInfo>();
            foreach (var scanResult in scanResults)
            {
                foreach (var badFile in scanResult.Value.BadFiles)
                {
                    if (!files.Any(f => f.Sha1 == badFile.Sha1))
                    {
                        files.Add(new ValkyrieFileInfo() { Ip = scanResult.Value.IpAddress, Path = badFile.Path, Sha1 = badFile.Sha1 });
                    }
                }
            }
            return files;
        }

        public IDomainDiscoverer GetDomainDiscoverer(string ldapPath, bool fromRoot, string domain, string login, string password)
        {
            IServiceCommand<FindTargetsResponse> command = GetDomainDiscoveryCommand(fromRoot, ldapPath, domain, login, password);
            return new DomainDiscoverer(command);
        }

        private IServiceCommand<FindTargetsResponse> GetDomainDiscoveryCommand(bool fromRoot, string ldapPath, string domain, string login, string password)
        {
            FindTargetsRequest findTargetsRequest = new FindTargetsRequest();
            DomainSearchCriterion searchCriterion = new DomainSearchCriterion();

            if (fromRoot)
                searchCriterion.FromRoot = true;
            else
                if (String.IsNullOrEmpty(ldapPath))
                    throw new ArgumentException("Ldap path is not specified.");
            searchCriterion.Domain = domain;
            searchCriterion.Login = login;
            searchCriterion.Password = password;
            searchCriterion.ParentAddress = ldapPath;

            findTargetsRequest.SearchCriterion = searchCriterion;
            return _commandFactory.CreateCommand<FindTargetsResponse>(findTargetsRequest);
        }

        public IWorkgroupDiscoverer GetWorkgroupDiscoverer(string workgroupName)
        {
            IServiceCommand<FindTargetsResponse> command = GetWorkgroupDiscoveryCommand(workgroupName);
            return new WorkgroupDiscoverer(command);
        }

        private IServiceCommand<FindTargetsResponse> GetWorkgroupDiscoveryCommand(string workgroupName)
        {
            FindTargetsRequest findTargetsRequest = new FindTargetsRequest();
            WorkgroupSearchCriterion searchCriterion = new WorkgroupSearchCriterion();

            searchCriterion.WorkgroupName = workgroupName;

            findTargetsRequest.SearchCriterion = searchCriterion;
            return _commandFactory.CreateCommand<FindTargetsResponse>(findTargetsRequest);
        }

        public IIpAddressDiscoverer GetIpAddressDiscoverer(EndpointAddressCollection addresses)
        {
            IServiceCommand<FindTargetsResponse> command = GetIpAddressDiscoveryCommand(addresses);
            return new IpAddressDiscoverer(command);
        }

        private IServiceCommand<FindTargetsResponse> GetIpAddressDiscoveryCommand(EndpointAddressCollection addresses)
        {
            FindTargetsRequest findTargetsRequest = new FindTargetsRequest();
            EndpointSearchCriterion searchCriterion = new EndpointSearchCriterion();

            searchCriterion.EndpointAddresses = addresses;

            findTargetsRequest.SearchCriterion = searchCriterion;
            return _commandFactory.CreateCommand<FindTargetsResponse>(findTargetsRequest);
        }

        public DeploymentTargetsCollection DiscoverDomain(string ldapPath, bool fromRoot, string domain, string login, string password)
        {
            IServiceCommand<FindTargetsResponse> command = GetDomainDiscoveryCommand(fromRoot, ldapPath, domain, login, password);
            command.Execute();
            return command.Response.Targets;
        }

        public IDomainTargetsResolver GetDomainTargetsResolver(DeploymentTargetsCollection targets, string login, string password)
        {
            IServiceCommand<GetTargetsSummaryResponse> command = GetDomainTargetsResolverCommand(targets, login, password);
            return new DomainTargetsResolver(command);
        }

        private IServiceCommand<GetTargetsSummaryResponse> GetDomainTargetsResolverCommand(DeploymentTargetsCollection targets, string login, string password)
        {
            if (targets == null || targets.Count == 0)
                throw new ArgumentException("Targets list is empty.");

            GetTargetsSummaryRequest request = new GetTargetsSummaryRequest { Targets = targets, DomainAccessLogin = login, DomainAccessPassword = password };
            return _commandFactory.CreateCommand<GetTargetsSummaryResponse>(request);
        }

        public DeploymentTargetsCollection CheckTargetsAvailability(DeploymentTargetsCollection targets, string login, string password)
        {
            var command = GetDomainTargetsResolverCommand(targets, login, password);
            command.Execute();
            return command.Response.Targets;
        }

        public IAptScanner GetAptScanner(DeploymentTargetsCollection targets, string login, string password, string domain, DevicesSourceDescription devicesSourceDescr, string scanScope = null)
        {
            if (scanScope == null)
                scanScope = Properties.Settings.Default.ScanScope;

            DeployAgentRequest request = new DeployAgentRequest
            {
                Targets = targets,
                Login = login,
                Password = password,
                ScanScope = scanScope,
                Domain = domain,
                DevicesSourceDescription = devicesSourceDescr
            };
            var command = _commandFactory.CreateCommand<DeployAgentResponse>(request);
            var scanner = new AptScanner(command, _descentSubSystem);
            scanner.ScanStarted += OnScanStarted;
            scanner.ScanCompleted += OnScanCompleted;
            return scanner;
        }

        private void OnScanStarted(object sender, ScanStartedEventArgs e)
        {
            ValkyrieAnalysisResult = null; 
        }

        private void OnScanCompleted(object sender, ScanCompletedEventArgs e)
        {
            AptScanner scanner = sender as AptScanner;
            if (scanner != null)
            {
                scanner.ScanStarted -= OnScanStarted;
                scanner.ScanCompleted -= OnScanCompleted;
            }
        }

        public IAptReportBuilder GetAptReportBuilder()
        {
            IAptReportBuilder aptReportBuilder = new AptReportBuilder(_descentSubSystem, this);
            return aptReportBuilder;
        }

        public ScanResult GetScanResultByTargetUid(Guid targetUid)
        {
            ScanResult scanResult = null;
            var results = _descentSubSystem.GetFirstScanResults();
            if (results != null)
            {
                results.ScanResults.TryGetValue(targetUid, out scanResult);
            }

            return scanResult;
        }

        public void StopScan(Guid targetUid)
        {
            _descentSubSystem.AbortThreadByTargetUid(targetUid);
        }

        public void StopScan()
        {
            _descentSubSystem.AbortAllThreads();
            _valkyrieClient.AnalyzeAsyncCancel();
        }


        public RegisterWithResponse Register(string camLicenseId, string camUserName, string camPassword, NetworkCredential proxyCredential)
        {
            var camRegistration = new CamRegistration(camLicenseId, camUserName, camPassword);

            RegisterWithResponse regInfo = _valkyrieClient.Register(camRegistration, proxyCredential.UserName, proxyCredential.Password);

            return regInfo;
        }

        public void StartValkyrieAnalysisAsync(string login, string password, string apiKey )
        {
            var results = _descentSubSystem.GetFirstScanResults().ScanResults;
            var distinctFileList = GetDistinctFileInfo(results);

            _valkyrieClient.AnalyzeAsync(distinctFileList, login, password,results.Count, apiKey);           
        }

        public List<FileResult> ValkyrieAnalysisResult { get; set; }

        public string GetValkyrieDetailsLink()
        {
            return _valkyrieClient.GetValkyrieDetailsLink();
        }

        public int GetDistinctFileCount()
        {
            var results = _descentSubSystem.GetFirstScanResults().ScanResults;
            var files = GetDistinctFileInfo(results);
            return files.Count;
        }
    }

}
