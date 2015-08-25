using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using APTAT.Core.Descent;
using APTAT.Core.Facade.Descent.Commands;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using APTAT.Core.Facade.Descent.MessageContracts;
using APTAT.Core.Reports;

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

    internal class AptScanner : IAptScanner
    {
        private AptScanOperationContext _scanOperationContext;
        private IServiceCommand<DeployAgentResponse> _command;
        private readonly IDescentSubSystem _descentSubSystem;
        public event EventHandler<ScanStartedEventArgs> ScanStarted;
        public event EventHandler<ScanCompletedEventArgs> ScanCompleted;
        public event EventHandler<ScanInProgressEventArgs> ScanInProgress;

        internal AptScanner(IServiceCommand<DeployAgentResponse> command, IDescentSubSystem descentSubSystem)
        {
            _command = command;
            _descentSubSystem = descentSubSystem;
            if (_descentSubSystem != null)
                _descentSubSystem.DescentCompleted += OnDescentCompleted;
        }

        private void OnDescentCompleted(object sender, DescentCompletedEventArgs e)
        {
            _descentSubSystem.DescentCompleted -= OnDescentCompleted;
            if (_scanOperationContext != null)
            {
                _scanOperationContext.OperationProgressChanged -= scanOperationContext_OperationProgressChanged;
            }

            OnScanCompleted(new ScanCompletedEventArgs()
            {
                OperationUid = e.OperationUid,
                CalceledCount = e.Context.CanceledTargetsCount,
                FailedCount = e.Context.FailedTargetsCount,
                NotPerformedCount = e.Context.NotPerformedTargetsCount,
                SucceededCount = e.Context.SucceededTargetsCount,
                TotalCount = e.Context.TotalTargetsCount,
                FilesStatistics = e.Context.CalculateFilesStatistics()
            });
        }

        public void BeginScan(object state = null)
        {
            if (_command != null)
            {
                Action action = _command.Execute;
                Context context = new Context() { Action = action, State = state };
                action.BeginInvoke(BeginScanCallback, context);
            }
        }

        private void BeginScanCallback(IAsyncResult ar)
        {
            Context context = ar.AsyncState as Context;
            if (context == null)
                return;

            ScanStartedEventArgs args = new ScanStartedEventArgs();
            try
            {
                context.Action.EndInvoke(ar);
                args.OperationUid = _command.Response.OperationUid;
                args.State = context.State;

                _scanOperationContext = _descentSubSystem.GetScanResultsByOperationUid(args.OperationUid);
                if (_scanOperationContext != null)
                {
                    _scanOperationContext.OperationProgressChanged += scanOperationContext_OperationProgressChanged;
                }
            }
            catch (Exception ex)
            {
                _descentSubSystem.DescentCompleted -= OnDescentCompleted;

                args.Error = ex;
                args.State = context.State;
            }
            finally
            {
                OnScanStarted(args);
            }
        }

        void scanOperationContext_OperationProgressChanged(object sender, AptScanOperationContext.OperationProgressChangedEventArgs e)
        {
            OnScanInProgress(new ScanInProgressEventArgs() { OperationUid = _scanOperationContext.OperationUid, Percentage = e.Percentage });
        }

        protected virtual void OnScanStarted(ScanStartedEventArgs args)
        {
            EventHandler<ScanStartedEventArgs> temp = ScanStarted;
            if (temp != null)
                temp(this, args);
        }

        protected virtual void OnScanCompleted(ScanCompletedEventArgs args)
        {
            EventHandler<ScanCompletedEventArgs> temp = ScanCompleted;
            if (temp != null)
                temp(this, args);
        }

        protected virtual void OnScanInProgress(ScanInProgressEventArgs args)
        {
            EventHandler<ScanInProgressEventArgs> temp = ScanInProgress;
            if (temp != null)
                temp(this, args);
        }

        public IServiceCommand<DeployAgentResponse> Command
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


    public interface IAptReportBuilder
    {
        void BeginBuildReport(Guid operationUid, ReportType reportType, object state = null);
        void BeginBuildReport(AptScanOperationContext operationContext, ReportType reportType, object state = null);
        event EventHandler<ReportCompletedEventArgs> ReportCompleted;
    }

    internal class AptReportBuilder : IAptReportBuilder
    {
        private readonly IDescentSubSystem _descentSubSystem;
        private readonly ReportBuilder _reportBuilder = new ReportBuilder();

        public AptReportBuilder(IDescentSubSystem descentSubSystem)
        {
            _descentSubSystem = descentSubSystem;
        }

        /* FAKE DATA
        public void BeginBuildReport(Guid operationUid, ReportType reportType, object state = null)
        {
            //data = GetFakeData();
            var results = _descentSubSystem.GetScanResultsByOperationUid(operationUid);
            // if (results.Finished == DateTime.MinValue)
            //     throw new DataNotReadyException();
            BeginBuildReport(results, reportType, state);
        }

        public void BeginBuildReport(AptScanOperationContext operationContext, ReportType reportType, object state = null)
        {

            AptReportData data = GetFakeData();

            ReportBuilder.BuildReportDelegate action = new ReportBuilder.BuildReportDelegate(_reportBuilder.Build);
            Context context = new Context() { Action = action, State = state };
            action.BeginInvoke(data, reportType, BuildReportCallback, context);

        }
        */
        public void BeginBuildReport(Guid operationUid, ReportType reportType, object state = null)
        {
            //data = GetFakeData();
            var results = _descentSubSystem.GetScanResultsByOperationUid(operationUid);
            if (results.Finished == DateTime.MinValue)
                throw new DataNotReadyException();
            BeginBuildReport(results, reportType, state);
        }

        public void BeginBuildReport(AptScanOperationContext operationContext, ReportType reportType, object state = null)
        {
            if (operationContext != null)
            {
                AptReportData data = new AptReportData
                {
                    OperationUid = operationContext.OperationUid,
                    ScanStarted = operationContext.Started.ToLocalTime(),
                    ScanFinished = operationContext.Finished.ToLocalTime(),
                    TargetsSource = operationContext.TargetsSourceName,
                    TargetsFailed = operationContext.FailedTargetsCount,
                    TargetsNotPerformed = operationContext.NotPerformedTargetsCount,
                    TotalTargetsCount = operationContext.TotalTargetsCount,
                    TargetsSucceeded = operationContext.SucceededTargetsCount,
                    Targets = operationContext.ScanResults.Values.ToArray(),
                    DevicesSourceDescription = operationContext.DevicesSourceDescription
                };

                ReportBuilder.BuildReportDelegate action = new ReportBuilder.BuildReportDelegate(_reportBuilder.Build);
                Context context = new Context() { Action = action, State = state };
                action.BeginInvoke(data, reportType, BuildReportCallback, context);
            }
            else
            {
                throw new InvalidOperationException("Scan results not found.");
            }
        }

        private static AptReportData GetFakeData()
        {
            AptReportData data = new AptReportData();
            data.ScanFinished = DateTime.Now;
            data.ScanStarted = DateTime.Now.AddHours(-1);
            data.TargetsFailed = 1;
            data.TotalTargetsCount = 50;
            data.TargetsSource = "dev.comodo.od.ua";
            data.TargetsSucceeded = 49;

            var unknown = new Module() { Path = @"C:\Program Files (x86)\COMODO\Comodo Internet Security\cfp.exe", Accessed = DateTime.Now, Attributes = "", Cloud = CloudVerdict.Unknown, Copyright = "Microsoft", Created = DateTime.Now.AddYears(-1), Description = "blah-blah-blah", Id = "1", Modified = DateTime.Now, Product = "Windows", Sha1 = "432532907249321", Signer = "Microsoft", Size = 300, Version = "1.0.0.0" };
            var malware = new Module() { Path = @"C:\Windows\svch0st.exe", Accessed = DateTime.Now, Attributes = "", Cloud = CloudVerdict.Malware, Copyright = "N/A", Created = DateTime.Now.AddYears(-2), Description = "blah-blah-blah", Id = "2", Modified = DateTime.Now, Product = "Windows", Sha1 = "432532907249322", Signer = "Microsoft", Size = 777, Version = "1.0.0.0" };
            var svchost = new Module() { Path = @"D:\svch0st.exe", Accessed = DateTime.Now, Attributes = "", Cloud = CloudVerdict.Malware, Copyright = "N/A", Created = DateTime.Now.AddYears(-3), Description = "blah-blah-blah", Id = "4", Modified = DateTime.Now, Product = "Windows", Sha1 = "432532907249322", Signer = "Microsoft", Size = 777, Version = "1.0.0.0" };
            var malware2 = new Module() { Path = @"C:\Windows\expl0rer.exe", Accessed = DateTime.Now, Attributes = "", Cloud = CloudVerdict.Malware, Copyright = "N/A", Created = DateTime.Now.AddYears(-5), Description = "blah-blah-blah", Id = "3", Modified = DateTime.Now, Product = "Windows", Sha1 = "432532907249323", Signer = "Microsoft", Size = 777, Version = "1.0.0.0" };

            ScanResult result = new ScanResult() { HostName = "agubernskyy-new", IpAddress = "10.100.65.223", OrganizationUnit = "Dev", MalwareFilesCount = 3, UnknownFilesCount = 1, SafeFilesCount = 303, TotalFilesCount = 307, ResultType = ScanResultType.Completed, TargetUid = new Guid("{D0221479-370A-4429-AB3A-FAE96D11AD02}") };
            result.SetAptReportInstance(new AptReport() { Modules = new Module[] { /*new Module() { Path = "kernel32.dll", Cloud = CloudVerdict.Unknown },*/ unknown, malware, svchost, malware2 } });
            ScanResult result2 = new ScanResult() { HostName = "dkolomiyets", OrganizationUnit = "Dev", IpAddress = "10.100.65.3", MalwareFilesCount = 2, UnknownFilesCount = 1, SafeFilesCount = 337, TotalFilesCount = 340, ResultType = ScanResultType.Completed, TargetUid = new Guid("{911DD54A-9714-4718-8DCC-2AA15A18E6A8}") };
            result2.SetAptReportInstance(new AptReport() { Modules = new Module[] { malware, unknown, malware2 } });
            ScanResult result3 = new ScanResult() { HostName = "cesmuser", OrganizationUnit = "QA", IpAddress = "10.100.65.225", MalwareFilesCount = 0, UnknownFilesCount = 0, SafeFilesCount = 100, TotalFilesCount = 100, ResultType = ScanResultType.Completed, TargetUid = new Guid("{09B449D0-366B-47CF-A6CD-8EF15DAF9B9F}") };
            result3.SetAptReportInstance(new AptReport() { Modules = new Module[] { /*new Module() { Path = "user32.dll", Cloud = CloudVerdict.Safe }*/ } });

            data.Targets = new ScanResult[] { result, result2, result3 };
            return data;
        }

        private void BuildReportCallback(IAsyncResult ar)
        {
            Context context = ar.AsyncState as Context;
            if (context == null)
                return;

            ReportCompletedEventArgs args = new ReportCompletedEventArgs();
            try
            {
                string fileName = context.Action.EndInvoke(ar);
                args.FileName = fileName;
                args.State = context.State;
            }
            catch (Exception ex)
            {
                args.Error = ex;
            }
            finally
            {
                OnReportCompleted(args);
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);
            }
        }

        public event EventHandler<ReportCompletedEventArgs> ReportCompleted;

        protected virtual void OnReportCompleted(ReportCompletedEventArgs args)
        {
            EventHandler<ReportCompletedEventArgs> temp = ReportCompleted;
            if (temp != null)
                temp(this, args);
        }

        internal class Context
        {
            internal ReportBuilder.BuildReportDelegate Action { get; set; }
            internal object State { get; set; }
        }
    }

    public class ReportCompletedEventArgs : EventArgs
    {
        public object State { get; set; }
        public string FileName { get; set; }
        public Exception Error { get; set; }
    }

    public class AptAtManager
    {
        private readonly CommandFactory _commandFactory;
        private readonly IDescentSubSystem _descentSubSystem;
        private readonly IChangeTrackingSubSystem _changeTrackingSubSystem;        
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
            _changeTrackingSubSystem = catalog.Find<IChangeTrackingSubSystem>();

            if (listener != null)
                _changeTrackingSubSystem.RegisterListener(listener);
        }

        public void RegisterChangeTrackingListener(IChangeTrackingListener listener)
        {
            if (listener != null)
                _changeTrackingSubSystem.RegisterListener(listener);
        }

        public void UnregisterChangeTrackingListener(IChangeTrackingListener listener)
        {
            if (listener != null)
                _changeTrackingSubSystem.UnregisterListener(listener);
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
            return new AptScanner(command, _descentSubSystem);
        }

        public IAptReportBuilder GetAptReportBuilder()
        {
            IAptReportBuilder aptReportBuilder = new AptReportBuilder(_descentSubSystem);
            return aptReportBuilder;
        }

        public AptScanOperationContext GetOperationContext(Guid operationUid)
        {
            return _descentSubSystem.GetScanResultsByOperationUid(operationUid);
        }

        public ScanResult GetScanResultByOperationAndTargetUid(Guid operationUid, Guid targetUid)
        {
            ScanResult scanResult = null;
            var results = _descentSubSystem.GetScanResultsByOperationUid(operationUid);
            if (results != null)
            {
                results.ScanResults.TryGetValue(targetUid, out scanResult);
            }
            return scanResult;
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
        }

        public ConcurrentDictionary<Guid,ScanResult> GetFirstScanReslts()
        {
            return _descentSubSystem.GetFirstScanResults().ScanResults;
        }
    }


}
