using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.DeploymentCore.Descent;

namespace APTAT.Core.Descent.Apt
{
    public interface IAptDescentProgressItem : IDescentProgressItem
    {
        AptScanStage ScanStage { get; }
        int TotalFilesCount { get; }
        int UnknownFilesCount { get; }
        int FailedAtNodeCount { get; set; }
        int MalwaresCount { get; }
        int SafeFilesCount { get; }
        int ErrorFilesCount { get; }
    }
}
