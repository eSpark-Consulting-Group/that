using System;
using System.Collections.Generic;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;


namespace APTAT.Core.Apt
{
    internal class AptScanner : IAptScanner
    {
        private AptScanOperationContext _scanOperationContext;
        private readonly IDescentSubSystem _descentSubSystem;
        private readonly string _password;
        private readonly string _login;
        private readonly string _scanScope;
        private Guid _operationUid;
        private readonly IEnumerable<DeploymentTarget> _targets;
        private readonly string _udpPort;
        private readonly string _tcpPort;

        public event EventHandler<ScanStartedEventArgs> ScanStarted;
        public event EventHandler<ScanCompletedEventArgs> ScanCompleted;
        public event EventHandler<AptScanInProgressEventArgs> ScanInProgress;

        internal AptScanner(IDescentSubSystem descentSubSystem, IEnumerable<DeploymentTarget> targets, string login,
            string password, string scanScope, string tcpPort, string udpPort)
        {
            _descentSubSystem = descentSubSystem;
            _targets = targets;
            _login = login;
            _password = password;
            _scanScope = scanScope;
            _tcpPort = tcpPort;
            _udpPort = udpPort;
        }

        private void OnDescentCompleted(object sender, DescentEventArgs e)
        {
            if (_operationUid != e.OperationUid) return;
            _descentSubSystem.DescentCompleted -= OnDescentCompleted;
            if (_scanOperationContext != null)
            {
                _scanOperationContext.OperationProgressChanged -= scanOperationContext_OperationProgressChanged;
            }
            var aptScanCompletedEventArgs = e as AptScanCompletedEventArgs;
            if (aptScanCompletedEventArgs == null) return;

            OnScanCompleted(new ScanCompletedEventArgs()
            {
                OperationUid = e.OperationUid,
                CalceledCount = aptScanCompletedEventArgs.Context.CanceledTargetsCount,
                FailedCount = aptScanCompletedEventArgs.Context.FailedTargetsCount,
                NotPerformedCount = aptScanCompletedEventArgs.Context.NotPerformedTargetsCount,
                SucceededCount = aptScanCompletedEventArgs.Context.SucceededTargetsCount,
                TotalCount = aptScanCompletedEventArgs.Context.TotalTargetsCount,
                FilesStatistics = aptScanCompletedEventArgs.Context.CalculateFilesStatistics()
            });
        }

        public void BeginScan(object state = null)
        {
            Action action = StartScan;
            Context context = new Context() { Action = action, State = state };
            _descentSubSystem.DescentCompleted += OnDescentCompleted;
            action.BeginInvoke(BeginScanCallback, context);
        }

        private void StartScan()
        {
            Guid operationUid;
            _descentSubSystem.StartAptScan(_targets, _login, _password, out operationUid, 
                _scanScope, _tcpPort, _udpPort);
            _operationUid = operationUid;
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
                args.OperationUid = _operationUid;
                args.State = context.State;

                _scanOperationContext = _descentSubSystem.GetAptScanResultsByOperationUid(args.OperationUid);
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

        void scanOperationContext_OperationProgressChanged(object sender, AptScanOperationContext.AptOperationProgressChangedEventArgs e)
        {
            OnScanInProgress(new AptScanInProgressEventArgs()
            {
                OperationUid = _scanOperationContext.OperationUid, Percentage = e.Percentage,
                ProgressItems = e.ProgressItems
            });
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

        protected virtual void OnScanInProgress(AptScanInProgressEventArgs args)
        {
            EventHandler<AptScanInProgressEventArgs> temp = ScanInProgress;
            if (temp != null)
                temp(this, args);
        }

        internal class Context
        {
            internal Action Action { get; set; }
            internal object State { get; set; }
        }
    }
}
