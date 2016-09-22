using APTAT.Common;

namespace APTAT.DeploymentCore.Descent.Helpers
{
    public interface INetworkHelper
    {
        ImpersonationHelper.Logon32LogonType LogonType { get; set; }

        void GrantNetworkAccessToRemoteComputer(string userName, string password, string remotePath, bool impersonateAsNetworkService);
        void Release();
    }
}