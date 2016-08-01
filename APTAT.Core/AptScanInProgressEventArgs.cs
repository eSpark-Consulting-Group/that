using APTAT.Core.Descent;

namespace APTAT.Core
{
    public class AptScanInProgressEventArgs : ScanInProgressEventArgs
    {
        public DescentProgressItem[] ProgressItems { get; set; }
    }
}