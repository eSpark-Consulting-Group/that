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
        ScanKind ScanMethod { get; }
        IEnumerable<IAptScanResult> ScanResults { get; }
        event EventHandler<AptOperationProgressChangedEventArgs> OperationProgressChanged;
    }

    internal interface IAptScanOperationContextInternal : IAptScanOperationContext
    {
        void SetScanKind(ScanKind scanKind);
        void SetCounters(int SucceededTargetsCount, int FailedTargetsCount, int NotPerformedTargetsCount, int CanceledTargetsCount);
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
