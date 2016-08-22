using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.Threading.Tasks;
using APTAT.Common;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.DeploymentCore.Facade.Descent.MessageContracts;

namespace APTAT.DeploymentCore.Directory.Resolvers
{
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

        public async Task<TargetsResolverEventArgs> ResolveAsync(object state = null)
        {
            if (_command == null) throw new InvalidOperationException("command is null");

            Action action = _command.Execute;
            Context context = new Context() { Action = action, State = state };
            await Task.Run(() => action.Invoke());


            TargetsResolverEventArgs args = new TargetsResolverEventArgs();
            try
            {
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
            return args;

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
}
