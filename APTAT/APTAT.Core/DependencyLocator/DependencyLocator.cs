using APTAT.Common;
using APTAT.Core.Apt;

namespace APTAT.Core.DependencyLocator
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.RegisterSingleton<IAptScannerFactory, AptScannerFactory>();
            resolver.RegisterSingleton<IAptAtManager, AptAtManager>();
            resolver.Register<IFlsManager, FlsManager>();
            resolver.Register<IDistinctFileHelper, DistinctFileHelper>();
        }
    }
}
