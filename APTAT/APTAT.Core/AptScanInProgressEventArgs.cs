using APTAT.Core.Descent;
using APTAT.DeploymentCore.Descent;

namespace APTAT.Core
{
    public class AptScanInProgressEventArgs : ScanInProgressEventArgs
    {
        public DescentProgressItem[] ProgressItems { get; set; }
    }
}