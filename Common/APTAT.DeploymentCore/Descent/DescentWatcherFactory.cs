using System;
using APTAT.DeploymentCore.Properties;
using APTAT.Common;

namespace APTAT.DeploymentCore.Descent
{
    public sealed class DescentWatcherFactory : IDescentWatcherFactory
    {
        public IDescentWatcher CreateDescentWatcher(Action<Guid> onDescentTimeout)
        {
            IScheduler scheduler = CompositionHelper.Instance.Resolve<IScheduler>();
            return new DescentWatcher(Settings.Default.AgentDescentingTimeout, onDescentTimeout, scheduler);
        }
    }
    public interface IDescentWatcherFactory
    {
        IDescentWatcher CreateDescentWatcher(Action<Guid> onDescentTimeout);
    }
}