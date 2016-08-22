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
            Mock.Arrange(() => dependencyResolver.RegisterSingleton<IDescentSubSystem, DescentSubSystem>())
                .OccursOnce();
            Mock.Arrange(() => dependencyResolver.Register<IActiveDirectoryObserver, ActiveDirectoryObserver>())
                .OccursOnce();
            Mock.Arrange(() => dependencyResolver.Register<IWorkgroupObserver, WorkgroupObserver>())
                .OccursOnce();
            Mock.Arrange(() => dependencyResolver.Register<INetworkBrowser, NetworkBrowser>())
                .OccursOnce();
            Mock.Arrange(() => dependencyResolver.RegisterSingleton<ICommandFactory, CommandFactory>())
                .OccursOnce();
            Mock.Arrange(() => dependencyResolver.RegisterSingleton<ITargetsManager, TargetsManager>())
                .OccursOnce();
            Mock.Arrange(() => dependencyResolver.RegisterSingleton<IIPSatretegySelectionProvider, IPSatretegySelectionProvider>())
                .OccursOnce();
            target.RegisterDependencies(dependencyResolver);
        }
    }
}
