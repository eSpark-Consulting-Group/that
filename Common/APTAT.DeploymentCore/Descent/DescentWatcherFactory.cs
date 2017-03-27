using System;
using APTAT.DeploymentCore.Properties;

namespace APTAT.DeploymentCore.Descent
{
    public sealed class DescentWatcherFactory : IDescentWatcherFactory
    {
        public IDescentWatcher CreateDescentWatcher(Action<Guid> onDescentTimeout)
        {
            return new DescentWatcher(Settings.Default.AgentDescentingTimeout, onDescentTimeout);
        }

    }
    public interface IDescentWatcherFactory
    {
        IDescentWatcher CreateDescentWatcher(Action<Guid> onDescentTimeou);
    }
}