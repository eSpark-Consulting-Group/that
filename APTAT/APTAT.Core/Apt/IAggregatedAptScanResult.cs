using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Directory;
using System;
using System.Collections.Generic;

namespace APTAT.Core.Apt
{
    public interface IAggregatedAptScanResult
    {
        void MergeOperationContext(IAptScanOperationContext aptScanContext);
        bool IsOperationInProgress(Guid operationUid);
        Guid OperationUid { get; }
        int SucceededTargetsCount { get; }
        int FailedTargetsCount { get; }
        int NotPerformedTargetsCount { get; }
        int CanceledTargetsCount { get; }
        int TotalTargetsCount { get; }
        DateTime StartedTime { get; }
        IDevicesSourceDescription DeviceSourceDescription { get; }
        IEnumerable<IAptScanResult> ScanResults { get; }
        DateTime? CompletedTime { get; }
        IFilesStatistics CalculateFilesStatistics();
        int GetDistinctFilesCount();
    }
}
