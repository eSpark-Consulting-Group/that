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
            resolver.Register<IDescentSubSystem, DescentSubSystem>(true);
            resolver.Register<INetworkObserver, NetworkObserver>(false);
            resolver.Register<IActiveDirectoryObserver, ActiveDirectoryObserver>(false);
            resolver.Register<IWorkgroupObserver, WorkgroupObserver>(false);
            resolver.Register<INetworkBrowser, NetworkBrowser>(false); 
            resolver.Register<ICommandFactory, CommandFactory>(true);
            resolver.Register<ITargetsManager, TargetsManager>(true); 
            resolver.Register<IIPSatretegySelectionProvider, IPSatretegySelectionProvider>(true);
        }
    }
}
