using System;

namespace APTAT.DeploymentCore.Descent.Helpers
{
    public interface IRemoteHostInfo
    {
        string HostName { get; set; }
        bool IsCancelled { get;  }
        bool IsLocalComputer { get; }
        string Login { get; }
        string Password { get; }
        Guid TargetUid { get; }
    }
}