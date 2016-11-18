using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using APTAT.Common;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Messages;

namespace APTAT.Core.Descent.Apt
{
    public class AptScanOperationContext : DescentResult, IAptScanOperationContext
    {
        private readonly Guid _sessionId = Guid.NewGuid();

        public AptScanOperationContext(IEnumerable<IComputer> computers, int totalTargetCount,
            IDevicesSourceDescription devicesSourceDescription)
            : base(computers, devicesSourceDescription, totalTargetCount, new AptComputerDescentResultFactory())
        {
        }
        public DateTime Started { get { return StartTime; } }

        private readonly object _sync = new object();

        public override void Changed(params ChangesTrackingItem[] items)
        {
            base.Changed(items);
            if (items == null) return;
            OnOperationProgressChanged(new AptOperationProgressChangedEventArgs()
            {
                Percentage = CalculateTotalPercentage(),
                ProgressItems = items.Select(c => c.ChangedItem).OfType<IDescentProgressItem>().ToArray(),
                OperationUid = OperationUid
            });
        }

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

        public IAptScanResult GetAptResultByTargetUid(Guid targetUid)
        {
            var result = GetComputerDescentResultByTargetUid(targetUid);
            if (result is IAptScanResult)
            {
                return (IAptScanResult) result;
            }
            return null;
        }


        private double CalculateTotalPercentage()
        {
            var onlineScanResults = (from result in ResultsByUid.Values.OfType<IAptScanResult>()
                                     where result.ResultType != ScanResultType.NotPerformed
                                     select result).ToArray();

            var total = onlineScanResults.Count() * 100;
            var current = onlineScanResults.Sum(scanResult => scanResult.Percentage);

            return (double)current / total * 100d;
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
