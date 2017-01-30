using System;
using System.Collections.Generic;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using System.Collections.Concurrent;

namespace APTAT.Core.Apt
{
    internal class AptScanner : IAptScanner
    {
        private IAptScanOperationContext _scanOperationContext;
        private readonly IDescentSubSystem _descentSubSystem;
        private readonly IDevicesSourceDescription _devicesSourceDescription;
        private Guid _operationUid;
        private readonly IEnumerable<DeploymentTarget> _targets;
            
        
        private readonly ScanOptions _options;

        public event EventHandler<ScanStartedEventArgs> ScanStarted;
        public event EventHandler<ScanCompletedEventArgs> ScanCompleted;
        public event EventHandler<AptScanInProgressEventArgs> ScanInProgress;
        public event EventHandler<ComputerScanCompletedEventArgs> ComputerScanCompleted; 
        public IAptScanOperationContext ScanOperationContext { get { return _scanOperationContext; } }
        internal AptScanner(IDescentSubSystem descentSubSystem, IEnumerable<DeploymentTarget> targets, IDevicesSourceDescription devicesSourceDescription, ScanOptions options)
        {
            _devicesSourceDescription = devicesSourceDescription;
            _descentSubSystem = descentSubSystem;

            _targets = targets;
            _options = options;
        }

        private void OnDescentCompleted(object sender, DescentEventArgs e)
        {
            var aptScanCompletedEventArgs = e as AptScanCompletedEventArgs;
            if (aptScanCompletedEventArgs == null || e.OperationUid != _operationUid) return;
            
            _descentSubSystem.DescentCompleted -= OnDescentCompleted;
            if (_scanOperationContext != null)
            {
                _scanOperationContext.OperationProgressChanged -= scanOperationContext_OperationProgressChanged;
                _scanOperationContext.ComputerDescentCompleted -= ScanOperationContextOnComputerDescentCompleted;
            }

            OnScanCompleted(new ScanCompletedEventArgs()
            {
                OperationUid = _scanOperationContext.OperationUid,
                CanceledCount = _scanOperationContext.CanceledTargetsCount,
                FailedCount = _scanOperationContext.FailedTargetsCount,
                NotPerformedCount = _scanOperationContext.NotPerformedTargetsCount,
                SucceededCount = _scanOperationContext.SucceededTargetsCount,
                TotalCount = _scanOperationContext.TotalTargetsCount,
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
            var scanner = new AptDescentScanner(_targets, _options.Login, _options.Password, _devicesSourceDescription, _options);
            _descentSubSystem.StartScan(scanner, out operationUid);
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
                args.RescanAttempt = _options.IsRescan;
                args.State = context.State;
               
                _scanOperationContext = (IAptScanOperationContext)_descentSubSystem.GetDescentResultsByOperationUid(args.OperationUid);
                if (_scanOperationContext != null)
                {
                    _scanOperationContext.OperationProgressChanged += scanOperationContext_OperationProgressChanged;
                    _scanOperationContext.ComputerDescentCompleted += ScanOperationContextOnComputerDescentCompleted;                    
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

        private void ScanOperationContextOnComputerDescentCompleted(object sender, IComputerDescentResult computerDescentResult)
        {
            var result = computerDescentResult as IAptScanResult;
            if (result != null)
            {
                OnComputerScanCompleted(new ComputerScanCompletedEventArgs(_scanOperationContext.OperationUid, 
                    result));
            }
        }

        void scanOperationContext_OperationProgressChanged(object sender, AptOperationProgressChangedEventArgs e)
        {
            OnScanInProgress(new AptScanInProgressEventArgs()
            {
                OperationUid = _scanOperationContext.OperationUid, 
                Percentage = e.Percentage,
                ProgressItems = e.ProgressItems
            });
        }

        protected virtual void OnScanStarted(ScanStartedEventArgs args)
        {
            ScanStarted?.Invoke(this, args);
        }

        protected virtual void OnComputerScanCompleted(ComputerScanCompletedEventArgs args)
        {
            ComputerScanCompleted?.Invoke(this, args);
        }

        protected virtual void OnScanCompleted(ScanCompletedEventArgs args)
        {
            ScanCompleted?.Invoke(this, args);
        }

        protected virtual void OnScanInProgress(AptScanInProgressEventArgs args)
        {
            ScanInProgress?.Invoke(this, args);
        }
        internal class Context
        {
            internal Action Action { get; set; }
            internal object State { get; set; }
        }
        internal class ScanContextActionArgs
        {
            public IEnumerable<DeploymentTarget> Targets { get; set; }

            public string Login { get; set; }

            public string Password { get; set; }
        }
    }
}