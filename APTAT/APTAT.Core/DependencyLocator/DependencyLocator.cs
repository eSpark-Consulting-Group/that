using APTAT.Common;
using APTAT.Core.Apt;
using APTAT.Core.CamLicense;

namespace APTAT.Core.DependencyLocator
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.RegisterSingleton<ICamLicenseProvider, CamLicenseProvider>();
            resolver.RegisterSingleton<IAptScannerFactory, AptScannerFactory>();
            resolver.RegisterSingleton<IAptAtManager, AptAtManager>();
            resolver.RegisterSingleton<IFlsManager, FlsManager>();
            resolver.RegisterSingleton<IDistinctFileHelper, DistinctFileHelper>();
        }
    }
}
