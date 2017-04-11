using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.Common;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.DeploymentCore.Facade.Descent.MessageContracts;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Facade.DescentTest.CommandsTests
{
    public class FindWorkgroupTargetsCommandTests
    {
        public FindWorkgroupTargetsCommandTests()
        {
        }

        [Fact]
        public void ConstructorTest()
        {
            FindTargetsRequest request = new FindTargetsRequest();

            var networkBrowserMock = Mock.Create<INetworkBrowser>(Behavior.Strict);
            FindWorkgroupTargetsCommand target = new FindWorkgroupTargetsCommand(networkBrowserMock, request);

            Assert.Equal(request, target.Request);
        }

        [Fact]
        public void ExecuteTest()
        {
            WorkgroupSearchCriterion criteria = new WorkgroupSearchCriterion();
            criteria.WorkgroupName = "test";
            FindTargetsRequest request = new FindTargetsRequest();
            request.SearchCriterion = criteria;

            var networkBrowserMock = Mock.Create<INetworkBrowser>(Behavior.Strict);
            FindWorkgroupTargetsCommand target = new FindWorkgroupTargetsCommand(networkBrowserMock, request);

            var computers = new List<string>();
            for (int i = 0; i < 5; i++)
                computers.Add(Guid.NewGuid().ToString());

            networkBrowserMock
                .Arrange(browser => browser.GetNetworkComputers(ServerType.All, criteria.WorkgroupName))
                .Returns(new NetworkComputersResult()
                { Result = NetServerEnumResults.NERR_Success, Computers = computers })
                .MustBeCalled();

            target.Execute();
            var response = target.Response;
            Assert.Equal(computers, response.Targets.Select(t => ((WorkgroupTarget)t).Name));
        }

        [Fact]
        public void ExecuteShouldThrowFaultExceptionWhenErrorOccurredDuringObservingTest()
        {
            WorkgroupSearchCriterion criteria = new WorkgroupSearchCriterion();
            criteria.WorkgroupName = "test";
            FindTargetsRequest request = new FindTargetsRequest();
            request.SearchCriterion = criteria;
            var networkBrowserMock = Mock.Create<INetworkBrowser>(Behavior.Strict);
            FindWorkgroupTargetsCommand target = new FindWorkgroupTargetsCommand(networkBrowserMock, request);

            networkBrowserMock
                .Arrange(browser => browser.GetNetworkComputers(ServerType.All, criteria.WorkgroupName))
                .Returns(new NetworkComputersResult()
                {
                    Result = NetServerEnumResults.ERROR_NO_BROWSER_SERVERS_FOUND ,
                    Computers = null
                }).MustBeCalled();

            Assert.Throws<InvalidOperationException>(() =>
            {
                target.Execute();
            });
        }
    }
}
