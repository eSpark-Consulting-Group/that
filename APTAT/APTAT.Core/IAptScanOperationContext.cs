using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using APTAT.Core.Descent.Apt;
using APTAT.Core.Descent.Wmi;
using APTAT.DeploymentCore.Descent;


namespace APTAT.Core.Descent
{
    public interface IAptScanOperationContext : IDescentQueryResult
    {
        int SucceededTargetsCount { get; }
        int FailedTargetsCount { get; }
        int NotPerformedTargetsCount { get; }
        int CanceledTargetsCount { get; }
        DateTime Started { get; }
        IEnumerable<IAptScanResult> ScanResults { get; }

        event EventHandler<AptOperationProgressChangedEventArgs> OperationProgressChanged;
        IFilesStatistics CalculateFilesStatistics();
        void CalculateOperationStatistics();

    }

    public interface IAptScanMergeableOperationContext : IAptScanOperationContext
    {
        Action<IDescentProgressItem> MergeOperationProgressAction { get; set; }
        Action<IOperationDescentResult> MergeOperationResultAction { get; set; }

        void MergeOperationProgress(IDescentProgressItem progress, Func<Guid, IComputerDescentResult> func);

        void MergeOperationResult(IOperationDescentResult result, Func<Guid, IComputerDescentResult> func);
    }

    public class AptOperationProgressChangedEventArgs : OperationProgressChangedEventArgs
    {
        public IDescentProgressItem[] ProgressItems { get; set; }
    }
    public interface IWmiScanOperationContext : IDescentQueryResult
    {
        ConcurrentDictionary<Guid, WmiScanResult> ScanResults { get; }
    }
}
