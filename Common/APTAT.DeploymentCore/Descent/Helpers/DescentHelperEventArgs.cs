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

    public class DescentConnectedEventArgs : DescentHelperEventArgs
    {
        public string HostName { get; set; }
        public DescentConnectedEventArgs(Guid targetUid, string hostName)
            : base (targetUid)
        {
            HostName = hostName;
        }
    }
}
