using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace APTAT.Core.Apt
{
    public class AggregatedAptScanResult : IAggregatedAptScanResult
    {
        readonly IAptScanOperationContext _baseOperationContext;
        readonly ConcurrentDictionary<Guid,IAptScanResult> _scanResults;
        public AggregatedAptScanResult(IAptScanOperationContext aptScanContext)
        {
            _baseOperationContext = aptScanContext;
            OperationUid = aptScanContext.OperationUid;
            SucceededTargetsCount = aptScanContext.SucceededTargetsCount;
            FailedTargetsCount = aptScanContext.FailedTargetsCount;
            NotPerformedTargetsCount = aptScanContext.NotPerformedTargetsCount;
            CanceledTargetsCount = aptScanContext.CanceledTargetsCount;
            TotalTargetsCount = aptScanContext.TotalTargetsCount;
            StartedTime = aptScanContext.Started;
            DeviceSourceDescription = aptScanContext.DevicesSourceDescription;
            CompletedTime = aptScanContext.CompletedTime;
            _scanResults = new ConcurrentDictionary<Guid, IAptScanResult> 
                (aptScanContext.ScanResults.ToDictionary(r => r.TargetUid));
            _baseOperationContext.DescentCompleted += OnOperationContextDescentCompleted;
            _baseOperationContext.ComputerDescentCompleted += OnOperationContextComputerDescentCompleted;
        }

        private void OnOperationContextDescentCompleted(object sender, IDescentResult descentResult)
        {
            var operationResult = sender as IAptScanOperationContext;
            operationResult.DescentCompleted -= OnOperationContextDescentCompleted;
            CompletedTime = operationResult.CompletedTime;
        }

        private void OnOperationContextComputerDescentCompleted(object sender, IComputerDescentResult computerDescentResult)
        {
            CalculateOperationStatistics();
        }

        public void MergeOperationContext(IAptScanOperationContext aptScanContext)
        {
            foreach(var sr in aptScanContext.ScanResults)
                _scanResults.AddOrUpdate(sr.TargetUid, sr, (key, oldValue) => sr);
            _baseOperationContext.DescentCompleted += OnOperationContextDescentCompleted;
            _baseOperationContext.ComputerDescentCompleted += OnOperationContextComputerDescentCompleted;
        }

        public IFilesStatistics CalculateFilesStatistics()
        {
            return StatisticsCalculator.CalculateFilesStatistics(ScanResults);
        }
        public int GetDistinctFilesCount()
        {
            return ScanResults.Where(sr => !sr.IsCanceled).SelectMany(sr => sr.BadFiles)
                .Distinct().Count();
        }
        public Guid OperationUid { get; private set; }
        public int SucceededTargetsCount { get; private set; }
        public int FailedTargetsCount { get; private set; }
        public int NotPerformedTargetsCount { get; private set; }
        public int CanceledTargetsCount { get; private set; }
        public int TotalTargetsCount { get; }
        public DateTime StartedTime { get; private set; }
        public IDevicesSourceDescription DeviceSourceDescription { get; private set; }
        public IEnumerable<IAptScanResult> ScanResults { get { return _scanResults.Values; } }
        public DateTime? CompletedTime { get; private set; }

        public void CalculateOperationStatistics()
        {
            int failed, notPerformed, succeeded, total, canceled;
            StatisticsCalculator.CalculateOperationStatistics(ScanResults, out failed, out notPerformed, out succeeded, out total, out canceled);
            SucceededTargetsCount = succeeded;
            FailedTargetsCount = failed;
            NotPerformedTargetsCount = TotalTargetsCount - total;
            CanceledTargetsCount = canceled;
        }
    }
}
