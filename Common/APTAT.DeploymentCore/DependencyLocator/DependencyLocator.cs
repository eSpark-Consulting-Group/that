using System;
using APTAT.Common;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.Commands;

namespace APTAT.DeploymentCore
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.RegisterSingleton<IDescentWatcherFactory, DescentWatcherFactory>();
            resolver.RegisterSingleton<IDescentSubSystem, DescentSubSystem>();
            resolver.Register<INetworkObserver, NetworkObserver>();
            resolver.Register<IActiveDirectoryObserver, ActiveDirectoryObserver>();
            resolver.Register<IWorkgroupObserver, WorkgroupObserver>();
            resolver.Register<INetworkBrowser, NetworkBrowser>(); 
            resolver.RegisterSingleton<ICommandFactory, CommandFactory>();
            resolver.RegisterSingleton<ITargetsManager, TargetsManager>(); 
            resolver.RegisterSingleton<IIPSatretegySelectionProvider, IPSatretegySelectionProvider>();
        }
    }
}
