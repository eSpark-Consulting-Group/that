using System;
using APTAT.Core.Descent;

namespace APTAT.Core
{
    public interface IAptScanner
    {
        IAptScanOperationContext ScanOperationContext { get; }
        void BeginScan(object state = null);
        event EventHandler<ScanStartedEventArgs> ScanStarted;
        event EventHandler<ScanCompletedEventArgs> ScanCompleted;
        event EventHandler<AptScanInProgressEventArgs> ScanInProgress;
        event EventHandler<ComputerScanCompletedEventArgs> ComputerScanCompleted;
    }
}