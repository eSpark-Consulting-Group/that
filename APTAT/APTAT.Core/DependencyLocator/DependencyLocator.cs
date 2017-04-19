using APTAT.Common;
using APTAT.Core.Apt;
using APTAT.Core.CamLicense;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Reports;
using APTAT.Core.Storage;
using APTAT.Core.Valkyrie;
using APTAT.DeploymentCore.Descent;

namespace APTAT.Core.DependencyLocator
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        { 
            resolver.RegisterSingleton<ICamLicenseProvider, CamLicenseProvider>();
            resolver.RegisterSingleton<IAptScannerFactory, AptScannerFactory>();
            resolver.RegisterSingleton<IDescentScannerFactory, AptDescentScannerFactory>();
            resolver.RegisterSingleton<IAptFactory, AptFactory>();
            resolver.RegisterSingleton<IAptAtManager , AptAtManager>();

            resolver.Register<IRemoteFactory, RemoteFactory>();

            resolver.RegisterSingleton<IFlsManager, FlsManager>();
            resolver.RegisterSingleton<IFlsApiProvider, FlsApiProvider>();
            resolver.RegisterSingleton<IValkyrieClientEx, ValkyrieClientWithStorage>();
            
            resolver.RegisterSingleton<IComputerDescentResultFactory, AptComputerDescentResultFactory>();
            resolver.RegisterSingleton<IDistinctFileHelper, DistinctFileHelper>();
            resolver.RegisterSingleton<IStorageSubSystem, IValkyrieStorageSubSystem, StorageSubSystem>();
            resolver.RegisterSingleton<IReportBuilder, ReportBuilder>();
            resolver.RegisterSingleton<IFinalVerdictCalculator, FinalVerdictCalculator>();
            resolver.RegisterSingleton<IReportBuilderFactory, ReportBuilderFactory>();
            resolver.RegisterSingleton<IReportNamesCache, ReportNamesCache>();
        }
    }
}
