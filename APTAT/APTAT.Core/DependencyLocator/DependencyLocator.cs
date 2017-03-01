using APTAT.Common;
using APTAT.Core.Apt;
using APTAT.Core.CamLicense;
using APTAT.Core.Storage;

namespace APTAT.Core.DependencyLocator
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            APTAT.DAL.DependencyLocator.DependencyLocator d = new APTAT.DAL.DependencyLocator.DependencyLocator();
            d.RegisterDependencies(resolver);

            resolver.RegisterSingleton<ICamLicenseProvider, CamLicenseProvider>();
            resolver.RegisterSingleton<IAptScannerFactory, AptScannerFactory>();
            resolver.RegisterSingleton<IAptAtManager, AptAtManager>();
            resolver.RegisterSingleton<IFlsManager, FlsManager>();
            resolver.RegisterSingleton<IDistinctFileHelper, DistinctFileHelper>();
            resolver.RegisterSingleton<IStorageSubSystem, StorageSubSystem>();
        }
    }
}
