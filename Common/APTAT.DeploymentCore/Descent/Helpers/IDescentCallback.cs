using APTAT.DeploymentCore.Descent.Exceptions.Transport;

namespace APTAT.DeploymentCore.Descent.Helpers
{
    public interface IDescentCallback
    {
        void OnDescentCallback(DescentStatus status, int win32Error, string response);
        void SetDescentThread();
        bool IsCompleted { get; }
    }
}
