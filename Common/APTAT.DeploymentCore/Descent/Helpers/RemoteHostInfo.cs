using System;

namespace APTAT.DeploymentCore.Descent.Helpers
{
    public class RemoteHostInfo : IRemoteHostInfo
    {
        public RemoteHostInfo(Guid targetUid, string hostName, string login, 
            string password, bool isLocalComputer)
        {
            HostName = hostName;
            Login = login;
            Password = password;
            TargetUid = targetUid;
            IsLocalComputer = isLocalComputer;
        }

        public string HostName { get; set; }
        public string Login { get;  }
        public string Password { get;  }
        public bool IsLocalComputer { get;  }
        public Guid TargetUid { get; }
        public Boolean IsCancelled { get; set; }
    }
}
