using APTAT.Application.Services;
using APTAT.Common;

namespace APTAT.Application
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            CompositionHelper.Instance.Register<ITargetsDiscoveryService, TargetsDiscoveryService>();
            CompositionHelper.Instance.Register<IAptService, AptService>();
            CompositionHelper.Instance.Register<IStorageService, StorageService>();
            CompositionHelper.Instance.Register<IWmiService, WmiService>();
        }
    }
}
