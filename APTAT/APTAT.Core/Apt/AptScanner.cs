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
        private IAptScanMergeableOperationContext _mainScanOperationContext;
        private readonly IDescentSubSystem _descentSubSystem;
        private readonly IDevicesSourceDescription _devicesSourceDescription;

        private ConcurrentDictionary<Guid, object> _rescanOperations;
        private bool _mainOperationCompleted;
        
        private readonly ScanOptions _options;

        public event EventHandler<ScanStartedEventArgs> ScanStarted;
        public event EventHandler<ScanCompletedEventArgs> ScanCompleted;
        public event EventHandler<AptScanInProgressEventArgs> ScanInProgress;
        public event EventHandler<ComputerScanCompletedEventArgs> ComputerScanCompleted; 
        public IAptScanOperationContext ScanOperationContext { get { return _mainScanOperationContext; } }
        internal AptScanner(IDescentSubSystem descentSubSystem, IDevicesSourceDescription devicesSourceDescription, ScanOptions options)
        {
            _devicesSourceDescription = devicesSourceDescription;
            _descentSubSystem = descentSubSystem;

            _options = options;

            _rescanOperations = new ConcurrentDictionary<Guid, object>();
        }

        private void OnDescentCompleted(object sender, DescentEventArgs e)
        {
            var aptScanCompletedEventArgs = e as AptScanCompletedEventArgs;
            if (aptScanCompletedEventArgs == null) return;

            if (_rescanOperations.ContainsKey(e.OperationUid))
            {
                object val;
                if (_rescanOperations.TryRemove(e.OperationUid, out val) && !_mainOperationCompleted)
                    return;
            }
            else
                _mainOperationCompleted = true;

            if (_rescanOperations.Count > 0)
                return;

            _descentSubSystem.DescentCompleted -= OnDescentCompleted;
            if (_mainScanOperationContext != null)
            {
                _mainScanOperationContext.OperationProgressChanged -= scanOperationContext_OperationProgressChanged;
                _mainScanOperationContext.ComputerDescentCompleted -= ScanOperationContextOnComputerDescentCompleted;
            }

            OnScanCompleted(new ScanCompletedEventArgs()
            {
                OperationUid = _mainScanOperationContext.OperationUid,
                CanceledCount = _mainScanOperationContext.CanceledTargetsCount,
                FailedCount = _mainScanOperationContext.FailedTargetsCount,
                NotPerformedCount = _mainScanOperationContext.NotPerformedTargetsCount,
                SucceededCount = _mainScanOperationContext.SucceededTargetsCount,
                TotalCount = _mainScanOperationContext.TotalTargetsCount,
                FilesStatistics = _mainScanOperationContext.CalculateFilesStatistics()
            });
        }

        public void BeginScan(IEnumerable<DeploymentTarget> targets, string login, string password, bool rescanAttempt, object state = null)
        {
            var context = new ScanContext();
            context.Action = StartScan;
            context.State = state;
            context.RescanAttempt = rescanAttempt;

            var args = new ScanContextActionArgs();
            args.Targets = targets;
            args.Login = login;
            args.Password = password;
            args.GuidAcquired = _ => context.OperationGuid = _;

            _descentSubSystem.DescentCompleted -= OnDescentCompleted;
            _descentSubSystem.DescentCompleted += OnDescentCompleted;

            context.Action.BeginInvoke(args, BeginScanCallback, context);
        }

        private void StartScan(ScanContextActionArgs args)
        {
            var scanner = new AptDescentScanner(args.Targets, args.Login, args.Password, _devicesSourceDescription, _options);
            _descentSubSystem.StartScan(scanner, args.GuidAcquired);
        }

        private void BeginScanCallback(IAsyncResult ar)
        {
            var context = ar.AsyncState as ScanContext;
            if (context == null)
                return;

            ScanStartedEventArgs args = new ScanStartedEventArgs();
            try
            {
                context.Action.EndInvoke(ar);
                args.OperationUid = context.OperationGuid;
                args.RescanAttempt = context.RescanAttempt;
                args.State = context.State;

                var scanOperationContext = (IAptScanMergeableOperationContext)_descentSubSystem.GetDescentResultsByOperationUid(args.OperationUid);
                if (scanOperationContext != null)
                {
                    scanOperationContext.OperationProgressChanged += scanOperationContext_OperationProgressChanged;
                    scanOperationContext.ComputerDescentCompleted += ScanOperationContextOnComputerDescentCompleted;

                    if (!args.RescanAttempt)
                        _mainScanOperationContext = scanOperationContext;
                    else
                    {
                        scanOperationContext.MergeOperationProgressAction = _ => 
                            _mainScanOperationContext.MergeOperationProgress(_, 
                                scanOperationContext.GetComputerDescentResultByTargetUid);

                        scanOperationContext.MergeOperationResultAction = _ =>
                            _mainScanOperationContext.MergeOperationResult(_,
                                scanOperationContext.GetComputerDescentResultByTargetUid);

                        _rescanOperations.TryAdd(args.OperationUid, null);
                    }
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
                OnComputerScanCompleted(new ComputerScanCompletedEventArgs(_mainScanOperationContext.OperationUid, 
                    result));
            }
        }

        void scanOperationContext_OperationProgressChanged(object sender, AptOperationProgressChangedEventArgs e)
        {
            OnScanInProgress(new AptScanInProgressEventArgs()
            {
                OperationUid = _mainScanOperationContext.OperationUid, 
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

        internal class ScanContext
        {
            internal Action<ScanContextActionArgs> Action { get; set; }
            internal object State { get; set; }

            internal Guid OperationGuid { get; set; }
            internal bool RescanAttempt { get; set; }
        }

        internal class ScanContextActionArgs
        {
            public IEnumerable<DeploymentTarget> Targets { get; set; }

            public string Login { get; set; }

            public string Password { get; set; }

            public Action<Guid> GuidAcquired { get; set; }
        }

    }


}
