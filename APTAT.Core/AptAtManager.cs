using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using APTAT.Core.Descent;
using APTAT.Core.Facade.Descent.Commands;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.Core.Facade.Descent.MessageContracts;

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
    }
}
