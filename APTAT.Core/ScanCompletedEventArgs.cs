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
    public class ScanCompletedEventArgs : EventArgs
    {
        public Guid OperationUid { get; set; }
        public int TotalCount { get; set; }
        public int NotPerformedCount { get; set; }
        public int SucceededCount { get; set; }
        public int FailedCount { get; set; }
        public int CalceledCount { get; set; }

        public FilesStatistics FilesStatistics { get; set; }
    }
}