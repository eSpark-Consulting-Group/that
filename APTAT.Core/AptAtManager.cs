using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using APTAT.Core.Descent;
using APTAT.Core.Facade.Descent.Commands;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.Core.Facade.Descent.MessageContracts;
using Microsoft.Reporting.WinForms;

namespace APTAT.Core
{
    public interface IDomainDiscoverer
    {
        DeploymentTargetsCollection Discover();
        void BeginDiscover(object state = null);
        event EventHandler<DomainDiscovererEventArgs> Discovered;
    }

    internal class DomainDiscoverer : IDomainDiscoverer
    {
        private IServiceCommand<FindTargetsResponse> _command;
        public event EventHandler<DomainDiscovererEventArgs> Discovered;

        internal DomainDiscoverer()
        {
        }

        internal DomainDiscoverer(IServiceCommand<FindTargetsResponse> command)
        {
            _command = command;
        }

        public DeploymentTargetsCollection Discover()
        {
            if(_command != null)
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

            DomainDiscovererEventArgs args = new DomainDiscovererEventArgs();
            try
            {
                context.Action.EndInvoke(ar);
                args.Targets = _command.Response.Targets.ToArray();
                args.State = context.State;
            }
            catch (Exception ex)
            {
                args.Error = ex;
                args.State = context.State;
            }
            finally
            {
                OnDiscovered(args);
            }
        }

        protected virtual void OnDiscovered(DomainDiscovererEventArgs args)
        {
            EventHandler<DomainDiscovererEventArgs> temp = Discovered;
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

    public class DomainDiscovererEventArgs : EventArgs
    {
        public Exception Error { get; set; }
        public DeploymentTarget[] Targets { get; set; }
        public object State { get; set; }
    }


    public interface IDomainTargetsResolver
    {
        DeploymentTargetsCollection Resolve();
        void BeginResolve(object state = null);
        event EventHandler<DomainTargetsResolverEventArgs> TargetsResolved;
    }

    internal class DomainTargetsResolver : IDomainTargetsResolver
    {
        private IServiceCommand<GetTargetsSummaryResponse> _command;
        public event EventHandler<DomainTargetsResolverEventArgs> TargetsResolved;

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
                Context context = new Context(){ Action = action, State = state };
                action.BeginInvoke(BeginResolveCallback, context);
            }
        }

        private void BeginResolveCallback(IAsyncResult ar)
        {
            Context context = ar.AsyncState as Context;
            if (context == null)
                return;

            DomainTargetsResolverEventArgs args = new DomainTargetsResolverEventArgs();
            try
            {
                context.Action.EndInvoke(ar);
                args.Targets = _command.Response.Targets.ToArray();
                args.State = context.State;
            }
            catch (Exception ex)
            {
                args.Error = ex;
                args.State = context.State;
            }
            finally
            {
                OnTargetsResolved(args);
            }
        }

        protected virtual void OnTargetsResolved(DomainTargetsResolverEventArgs args)
        {
            EventHandler<DomainTargetsResolverEventArgs> temp = TargetsResolved;
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

    public class DomainTargetsResolverEventArgs : EventArgs
    {
        public Exception Error { get; set; }
        public DeploymentTarget[] Targets { get; set; }
        public object State { get; set; }
    }


    public interface IAptScanner
    {
        void BeginScan(object state = null);
        event EventHandler<ScanStartedEventArgs> ScanStarted;
    }

    internal class AptScanner : IAptScanner
    {
        private IServiceCommand<DeployAgentResponse> _command;
        public event EventHandler<ScanStartedEventArgs> ScanStarted;
        public event EventHandler<ScanCompletedEventArgs> ScanCompleted;

        internal AptScanner(IServiceCommand<DeployAgentResponse> command, IDescentSubSystem descentSubSystem)
        {
            _command = command;
            if(descentSubSystem != null)
                descentSubSystem.DescentCompleted += OnDescentCompleted;
        }

        private void OnDescentCompleted(object sender, DescentCompletedEventArgs e)
        {
            int total = 0;
            int notPerformed = 0;
            int succeeded = 0;
            int failed = 0;
            int calceled = 0;

            var results = e.Results.Values;
            total = results.Count;
            var groupped = from result in results group result by result.ResultType into temp select temp;
            foreach (var item in groupped)
            {
                if(item.Key == ScanResultType.NotPerformed)
                    notPerformed = item.Count();
                else if(item.Key == ScanResultType.Failed)
                    failed = item.Count();
                else if (item.Key == ScanResultType.Completed)
                    succeeded = item.Count();
                else if (item.Key == ScanResultType.Canceled)
                    calceled = item.Count();
            }

            OnScanCompleted(new ScanCompletedEventArgs(){OperationUid = e.OperationUid, CalceledCount = calceled, FailedCount = failed, NotPerformedCount = notPerformed, SucceededCount = succeeded, TotalCount = total});
        }

        public void BeginScan(object state = null)
        {
            if(_command != null)
            {
                Action action = _command.Execute;
                Context context = new Context(){Action = action, State = state};
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
            }
            catch (Exception ex)
            {
                args.Error = ex;
                args.State = context.State;
            }
            finally
            {
                OnScanStarted(args);
            }
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

            if(listener != null)
                _changeTrackingSubSystem.RegisterListener(listener);

            //LocalReport localReport = new LocalReport();
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

        public DeploymentTargetsCollection DiscoverDomain(string ldapPath, bool fromRoot, string domain, string login, string password)
        {
            IServiceCommand<FindTargetsResponse> command = GetDomainDiscoveryCommand(fromRoot, ldapPath, domain, login, password);
            command.Execute();
            return command.Response.Targets;
        }

        public IDomainTargetsResolver GetDomainTargetsResolver(DeploymentTargetsCollection targets)
        {
            IServiceCommand<GetTargetsSummaryResponse> command = GetDomainTargetsResolverCommand(targets);
            return new DomainTargetsResolver(command);
        }

        private IServiceCommand<GetTargetsSummaryResponse> GetDomainTargetsResolverCommand(DeploymentTargetsCollection targets)
        {
            if (targets == null || targets.Count == 0)
                throw new ArgumentException("Targets list is empty.");

            GetTargetsSummaryRequest request = new GetTargetsSummaryRequest {Targets = targets};
            return _commandFactory.CreateCommand<GetTargetsSummaryResponse>(request);
        }

        public DeploymentTargetsCollection CheckTargetsAvailability(DeploymentTargetsCollection targets)
        {
            var command = GetDomainTargetsResolverCommand(targets);
            command.Execute();
            return command.Response.Targets;
        }

        public IAptScanner GetAptScanner(DeploymentTargetsCollection targets, string login, string password)
        {
            DeployAgentRequest request = new DeployAgentRequest
                                             {
                                                 Targets = targets,
                                                 Login = login,
                                                 Password = password,
                                                 ScanScope = "processes|drivers|autoruns"
                                             };
            var command = _commandFactory.CreateCommand<DeployAgentResponse>(request);
            return new AptScanner(command, _descentSubSystem);
        }

        public ScanResult GetScanResultByOperationAndTargetUid(Guid operationUid, Guid targetUid)
        {
            ScanResult scanResult = null;
            var results = _descentSubSystem.GetScanResultsByOperationUid(operationUid);
            if(results != null)
            {
                results.TryGetValue(targetUid, out scanResult);
            }
            return scanResult;
        }

        public ScanResult GetScanResultByTargetUid(Guid targetUid)
        {
            ScanResult scanResult = null;
            var results = _descentSubSystem.GetFirstScanResults();
            if (results != null)
            {
                results.TryGetValue(targetUid, out scanResult);
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
    }
}
