using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using APTAT.Core.Apt;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.Reports;
using APTAT.Core.Valkyrie;
using ValkyrieVerdict = APTAT.Core.Valkyrie.ValkyrieVerdict;

namespace APTAT.Core
{
    public interface IAptScanner
    {
        void BeginScan(object state = null);
        event EventHandler<ScanStartedEventArgs> ScanStarted;
        event EventHandler<ScanCompletedEventArgs> ScanCompleted;
        event EventHandler<AptScanInProgressEventArgs> ScanInProgress;
    }
}