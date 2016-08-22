using APTAT.DeploymentCore.Descent;

namespace APTAT.Core.Descent.Apt
{
    public class AptScanCompletedEventArgs : DescentEventArgs
    {
        public AptScanOperationContext Context { get; set; }
    }
}
