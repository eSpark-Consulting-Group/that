using System;
using System.Diagnostics;
using System.DirectoryServices;
using APTAT.Common;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.DeploymentCore.Facade.Descent.MessageContracts;
using System.Threading.Tasks;

namespace APTAT.DeploymentCore.Directory.Discoverers
{
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

        public async Task<TargetsDiscovererEventArgs> DiscoverAwaitable()
        {
            if (_command != null)
            {
                Action action = _command.Execute;
                await Task.Run(() => _command.Execute());

                return DiscoverCallback();
            }

            return null;
        }

        private TargetsDiscovererEventArgs DiscoverCallback()
        {
            TargetsDiscovererEventArgs args = new TargetsDiscovererEventArgs();
            try
            {
                args.Targets = _command.Response.Targets.ToArray();
            }
            catch (DirectoryServicesCOMException ex)
            {
                LogHelper.Trace(TraceEventType.Error, "AptManager DiscoverCallback DirectoryServicesCOMException: {0}.", ex.Message);
                args.Error = new Exception(String.Format("An error occured while discovering Active Directory: {0}", ex.Message));
            }
            catch (Exception ex)
            {
                LogHelper.Trace(TraceEventType.Error, "AptManager DiscoverCallback error: {0}", ex.Message);
                args.Error = ex;
            }

            return args;
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
}
 