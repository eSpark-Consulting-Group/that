using APTAT.Core.Descent.Transport;

namespace APTAT.Core.Descent.Helpers
{
    public interface IDescentCallback
    {
        void OnDescentCallback(DescentStatus status, int win32Error, string response);
        void SetDescentThread();
        bool IsCompleted { get; }
    }
}
