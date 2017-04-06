using System;
using System.Linq;

namespace APTAT.DeploymentCore.Descent
{
    public interface IDescentWatcher : IDisposable
    {
        void AddIdentity(Guid agentIdentity);
        void ExcludeIdentity(Guid agentIdentity);
        TimeSpan DescentTimeout { get; }
        void NotifyProgress(Guid agentIdentity);
        void Start();
    }
}
