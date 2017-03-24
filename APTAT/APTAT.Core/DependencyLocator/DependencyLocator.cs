using APTAT.Common;
using APTAT.Core.Apt;
using APTAT.Core.CamLicense;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Reports;
using APTAT.Core.Storage;
using APTAT.Core.Valkyrie;
using System;

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

            resolver.RegisterSingleton<IDescentScannerFactory, AptDescentScannerFactory>();
            resolver.RegisterSingleton<IContextFactory, ContextFactory>();


            resolver.RegisterSingleton<IAptAtManager, AptAtManager>();
            resolver.RegisterSingleton<IFlsManager, FlsManager>();

            resolver.RegisterSingleton<IFlsApiProvider, FlsApiProvider>();
            resolver.RegisterSingleton<IValkyrieClientEx, ValkyrieClientWithStorage>();





            resolver.RegisterSingleton<IDistinctFileHelper, DistinctFileHelper>();
            resolver.RegisterSingleton<IStorageSubSystem, IValkyrieStorageSubSystem, StorageSubSystem>();
            resolver.RegisterSingleton<IReportBuilder, ReportBuilder>();
            resolver.RegisterSingleton<IFinalVerdictCalculator, FinalVerdictCalculator>();
            resolver.RegisterSingleton<IResultMergeHelper, ResultMergeHelper>(); 
            resolver.RegisterSingleton<IAptReportBuilder, AptReportBuilder>();
        }
    }
}
