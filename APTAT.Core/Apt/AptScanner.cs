using System;
using APTAT.Core.Descent;


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
        private readonly TargetUidComputerPair[] _targets;
        public event EventHandler<ScanStartedEventArgs> ScanStarted;
        public event EventHandler<ScanCompletedEventArgs> ScanCompleted;
        public event EventHandler<ScanInProgressEventArgs> ScanInProgress;

        internal AptScanner(IDescentSubSystem descentSubSystem, TargetUidComputerPair[] targets, string login,
            string password, string scanScope)
        {
            _descentSubSystem = descentSubSystem;
            if (_descentSubSystem != null)
                _descentSubSystem.DescentCompleted += OnDescentCompleted;
            _targets = targets;
            _login = login;
            _password = password;
            _scanScope = scanScope;
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
            Action action = StartScan;
            Context context = new Context() { Action = action, State = state };
            action.BeginInvoke(BeginScanCallback, context);
        }

        private void StartScan()
        {
            Guid operationUid;
            _descentSubSystem.StartAptScan(_targets, _login, _password, out operationUid, 
                _scanScope);
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

        internal class Context
        {
            internal Action Action { get; set; }
            internal object State { get; set; }
        }
    }
}
