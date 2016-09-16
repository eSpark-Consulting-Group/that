using APTAT.Common;
using DDT.Core.Helper;
using DDT.Core.Reports;

namespace DDT.Core
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.RegisterSingleton<IReportNamesCache, ReportNamesCache>();
            resolver.RegisterSingleton<IFileHelper, FileHelper>();            
            resolver.Register<IReportBuilder, ReportBuilder>();
            resolver.Register<IRegistryHelper, RegistryHelper>();
            resolver.Register<IProcessHelper, ProcessHelper>();
            resolver.Register<IEnvironmentHelper, EnvironmentHelper>();
            resolver.Register<ICheckNmapInstallation, CheckNmapInstallation>();
        }
    }
}
