using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;

namespace APTAT.Core.Descent.Apt
{
    public class AptScanOperationContext : DescentResult, IAptScanMergeableOperationContext
    {
        private readonly Guid _sessionId = Guid.NewGuid();

        public AptScanOperationContext(IEnumerable<IComputer> computers, int totalTargetCount,
            IDevicesSourceDescription devicesSourceDescription)
            : base(computers, devicesSourceDescription, totalTargetCount, new AptComputerDescentResultFactory())
        {
        }
        public DateTime Started { get { return StartTime; } }

        public override Guid SessionId
        {
            get { return _sessionId; }
        }

        public int SucceededTargetsCount { get;  set; }
        public int FailedTargetsCount { get; set; }
        public int NotPerformedTargetsCount { get; set; }
        public int CanceledTargetsCount { get; set; }

        public IEnumerable<IAptScanResult> ScanResults
        {
            get { return ResultsByUid.Values.OfType<IAptScanResult>(); }
        }

        protected override void UpdateStatus(IComputerDescentResultInternal result, IDescentProgressItem item)
        {
            base.UpdateStatus(result, item);

            if (MergeOperationProgressAction != null)
                MergeOperationProgressAction(item);
            else
                OnOperationProgressChanged(new AptOperationProgressChangedEventArgs()
                {
                    Percentage = CalculateTotalPercentage(),
                    ProgressItems = new[] { item },
                    OperationUid = OperationUid
                });
        }

        protected override void UpdateStatus(IComputerDescentResultInternal result, IOperationDescentResult item)
        {
            base.UpdateStatus(result, item);

            MergeOperationResultAction?.Invoke(item);
        }

        public Action<IDescentProgressItem> MergeOperationProgressAction { get; set; }

        public Action<IOperationDescentResult> MergeOperationResultAction { get; set; }

        public void MergeOperationProgress(IDescentProgressItem progress, Func<Guid, IComputerDescentResult> func)
        {
            var item = ResultsByUid.GetOrAdd(progress.ComputerUid,
                _ => (IComputerDescentResultInternal)func(_));

            if (item != null)
                UpdateStatus(item, progress);
        }

        public void MergeOperationResult(IOperationDescentResult result, Func<Guid, IComputerDescentResult> func)
        {
            ResultsByUid[result.ComputerUid] = (IComputerDescentResultInternal)func(result.ComputerUid);

            if (result.Succeeded)
            {
                SucceededTargetsCount++;
                FailedTargetsCount--;
            }
        }

        private double CalculateTotalPercentage()
        {
            var onlineScanResults = (from result in ResultsByUid.Values.OfType<IAptScanResult>()
                                     where result.ResultType != ScanResultType.NotPerformed
                                     select result).ToArray();

            var total = onlineScanResults.Count() * 100;
            var current = onlineScanResults.Sum(scanResult => scanResult.Percentage);

            return current / total * 100d;
        }

        public event EventHandler<AptOperationProgressChangedEventArgs> OperationProgressChanged;
        private void OnOperationProgressChanged(AptOperationProgressChangedEventArgs args)
        {
            OperationProgressChanged?.Invoke(this,args);
        }

        public IFilesStatistics CalculateFilesStatistics()
        {
            return StatisticsCalculator.Calculate(ScanResults);
        }

        private sealed class AptComputerDescentResultFactory : IComputerDescentResultFactory
        {
            public IComputerDescentResultInternal CreateComputerDescentResult(IComputer computer)
            {
                return new AptScanResult(computer);
            }
        }
    }
}
