using System;
using APTAT.Core.Descent;
using System.Collections.Generic;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Core
{
    public interface IAptScanner : IDisposable
    {
        IAptScanOperationContext ScanOperationContext { get; }
        void BeginScan(object state = null);

        event EventHandler<ScanStartedEventArgs> ScanStarted;
        event EventHandler<ScanCompletedEventArgs> ScanCompleted;
        event EventHandler<AptScanInProgressEventArgs> ScanInProgress;
        event EventHandler<ComputerScanCompletedEventArgs> ComputerScanCompleted;
    }
}