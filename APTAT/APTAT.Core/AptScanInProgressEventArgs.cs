using APTAT.Core.Descent;
using APTAT.DeploymentCore.Descent;

namespace APTAT.Core
{
    public class AptScanInProgressEventArgs : ScanInProgressEventArgs
    {
        public IDescentProgressItem[] ProgressItems { get; set; }
    }
}