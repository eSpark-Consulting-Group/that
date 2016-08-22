
using APTAT.Common;
using DDT.Application.Services;

namespace DDT.Application
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.Register<ITargetsDiscoveryService, TargetsDiscoveryService>();
            resolver.Register<IDeploymentService, DeploymentService>();
            resolver.Register<IReportService, ReportService>();
        }
    }
}
