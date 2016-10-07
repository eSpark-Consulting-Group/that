using APTAT.Application.Services;
using APTAT.Common;

namespace APTAT.Application
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.RegisterSingleton<ITargetsDiscoveryService, TargetsDiscoveryService>();
            resolver.RegisterSingleton<IAptService, AptService>();
            resolver.RegisterSingleton<IStorageService, StorageService>();
            resolver.RegisterSingleton<IFileBrowserService, FileBrowserService>();

        }
    }
}
