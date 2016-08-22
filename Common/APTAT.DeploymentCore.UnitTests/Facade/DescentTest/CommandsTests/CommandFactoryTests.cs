using System;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.DeploymentCore.Facade.Descent.MessageContracts;
using Xunit;
using Telerik.JustMock;

namespace APTAT.DeploymentCore.UnitTests.Facade.DescentTest.CommandsTests
{
    public class CommandFactoryTests : IDisposable
    {
        private ICommandFactory _target;

        public CommandFactoryTests()
        {
            var ip = Mock.Create<IIPSatretegySelectionProvider>(Behavior.Strict);
            var networkObserver = Mock.Create<INetworkObserver>(Behavior.Strict);
            INetworkBrowser networkBrowser = Mock.Create<INetworkBrowser>(Behavior.Strict);
            IActiveDirectoryObserver activeDirectoryObserver = Mock.Create<IActiveDirectoryObserver>(Behavior.Strict);
            IWorkgroupObserver workgroupObserver = Mock.Create<IWorkgroupObserver>(Behavior.Strict);
            _target = new CommandFactory(ip, networkObserver, networkBrowser, activeDirectoryObserver,
                workgroupObserver);
            //SubSystemFactoryHelper.InitMockProvider();
        }
        public void Dispose()
        {
            //SubSystemFactoryHelper.ResetProvider();
        }

        [Fact]
        public void CreateCommandResponseShouldThrowArgumentNullExceptionWhenRequestIsNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => { var command = _target.CreateCommand<FindTargetsResponse>(null); });
        }

        [Fact]
        public void CreateCommandResponseFindTargetsRequestEndpointSearchCriteriaTest()
        {
            var request = new FindTargetsRequest
            {
                SearchCriterion = new EndpointSearchCriterion()
            };

            var command = _target.CreateCommand<FindTargetsResponse>(request);

            Assert.IsType<FindEndpointTargetsCommand>(command);
            Assert.Equal(request, ((FindEndpointTargetsCommand)command).Request);
        }

        [Fact]
        public void CreateCommandResponseFindTargetsRequestADSearchCriteriaTest()
        {
            var request = new FindTargetsRequest
            {
                SearchCriterion = new DomainSearchCriterion()
            };

            var command = _target.CreateCommand<FindTargetsResponse>(request);

            Assert.IsType<FindDomainTargetsCommand>(command);
            Assert.Equal(request, ((FindDomainTargetsCommand)command).Request);
        }

        [Fact]
        public void CreateCommandResponseFindTargetsRequestWorkgroupSearchCriteriaTest()
        {
            var request = new FindTargetsRequest
            {
                SearchCriterion = new WorkgroupSearchCriterion()
            };

            var command = _target.CreateCommand<FindTargetsResponse>(request);
            Assert.IsType<FindWorkgroupTargetsCommand>(command);
            Assert.Equal(request, ((FindWorkgroupTargetsCommand)command).Request);
        }

        [Fact]
        public void CreateCommandResponseFindTargetsRequestUnknownCriteriaTest()
        {
            var request = new FindTargetsRequest
            {
                SearchCriterion = Mock.Create<TargetsSearchCriterion>()
            };

            Assert.Throws<ArgumentNullException>(() => { var command = _target.CreateCommand<FindTargetsResponse>(null); });
        }

        [Fact]
        public void CreateCommandResponseUnknownRequestTest()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var command = _target.CreateCommand<FindTargetsResponse>(new object());
            });
        }
    }
}
