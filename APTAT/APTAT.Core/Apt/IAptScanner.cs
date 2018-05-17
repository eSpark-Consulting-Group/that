using System;
using APTAT.Core.Descent;
using System.Collections.Generic;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using System.Security;

namespace APTAT.Core
{
    public interface IAptScanner : IDisposable
    {
        IAptScanOperationContext ScanOperationContext { get; }
        void BeginScan(string camLogin, SecureString camPass, string camLicenseID, object state = null);

        event EventHandler<ScanStartedEventArgs> ScanStarted;
        event EventHandler<ScanCompletedEventArgs> ScanCompleted;
        event EventHandler<AptScanInProgressEventArgs> ScanInProgress;
        event EventHandler<ComputerScanCompletedEventArgs> ComputerScanCompleted;
    }
}