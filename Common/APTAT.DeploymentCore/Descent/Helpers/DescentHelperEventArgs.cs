using System;

namespace APTAT.DeploymentCore.Descent.Helpers
{
    public class DescentHelperEventArgs : EventArgs
    {
        public DescentHelperEventArgs(Guid targetUid)
        {
            TargetUid = targetUid;
        }
        public Guid TargetUid { get; private set; }
    }
}
