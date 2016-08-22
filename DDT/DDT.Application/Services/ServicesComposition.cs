
using APTAT.Common;

namespace DDT.Application.Services
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
