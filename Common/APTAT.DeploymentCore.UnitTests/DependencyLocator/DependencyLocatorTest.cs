using APTAT.Common;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using Telerik.JustMock;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.DependencyLocator
{
    public class DependencyLocatorTest
    {
        [Fact]
        public void RegisterTest()
        {
            var target = new DeploymentCore.DependencyLocator();

            var dependencyResolver = Mock.Create<ICompositionHelper>(Behavior.Strict);
            Mock.Arrange(() => dependencyResolver.Register<INetworkObserver, NetworkObserver>());
            Mock.Arrange(() => dependencyResolver.Register<IDescentSubSystem, DescentSubSystem>(true))
                .OccursOnce();
            Mock.Arrange(() => dependencyResolver.Register<IActiveDirectoryObserver, ActiveDirectoryObserver>())
                .OccursOnce();
            Mock.Arrange(() => dependencyResolver.Register<IWorkgroupObserver, WorkgroupObserver>())
                .OccursOnce();
            Mock.Arrange(() => dependencyResolver.Register<INetworkBrowser, NetworkBrowser>())
                .OccursOnce();
            Mock.Arrange(() => dependencyResolver.Register<ICommandFactory, CommandFactory>(true))
                .OccursOnce();
            Mock.Arrange(() => dependencyResolver.Register<ITargetsManager, TargetsManager>(true))
                .OccursOnce();
            Mock.Arrange(() => dependencyResolver.Register<IIPSatretegySelectionProvider, IPSatretegySelectionProvider>(true))
                .OccursOnce();
            target.RegisterDependencies(dependencyResolver);
        }
    }
}
