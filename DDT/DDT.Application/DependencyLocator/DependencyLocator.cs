
using APTAT.Common;
using DDT.Application.Services;

namespace DDT.Application
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.Register<ITargetsDiscoveryService, TargetsDiscoveryService>(false);
            resolver.Register<IDeploymentService, DeploymentService>(false);
            resolver.Register<IReportService, ReportService>();
        }
    }
}
