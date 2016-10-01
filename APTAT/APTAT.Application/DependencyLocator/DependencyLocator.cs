using APTAT.Application.Services;
using APTAT.Common;

namespace APTAT.Application
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            CompositionHelper.Instance.RegisterSingleton<ITargetsDiscoveryService, TargetsDiscoveryService>();
            CompositionHelper.Instance.RegisterSingleton<IAptService, AptService>();
            CompositionHelper.Instance.RegisterSingleton<IStorageService, StorageService>();
            CompositionHelper.Instance.RegisterSingleton<IFileBrowserService, FileBrowserService>();

        }
    }
}
