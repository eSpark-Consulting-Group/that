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
        readonly INetworkBrowser _networkBrowserMock;
        public FindWorkgroupTargetsCommandTests()
        {
            _networkBrowserMock  = Mock.Create<INetworkBrowser>();
        }

        [Fact]
        public void ConstructorTest()
        {
            FindTargetsRequest request = new FindTargetsRequest();

            FindWorkgroupTargetsCommand target = new FindWorkgroupTargetsCommand(_networkBrowserMock,request);

            Assert.Equal(request, target.Request);
        }

        [Fact]
        public void ExecuteTest()
        {
            WorkgroupSearchCriterion criteria = new WorkgroupSearchCriterion();
            criteria.WorkgroupName = "test";
            FindTargetsRequest request = new FindTargetsRequest();
            request.SearchCriterion = criteria;
            List<string> expectedComps = new List<string>();
            expectedComps.Add("comp1");
            expectedComps.Add("comp2");
            expectedComps.Add("comp3");
            FindWorkgroupTargetsCommand target = new FindWorkgroupTargetsCommand(_networkBrowserMock, request);
            _networkBrowserMock
                .Arrange(browser => browser.GetNetworkComputers(ServerType.All, criteria.WorkgroupName, Arg.IsAny<List<string>>()))
                .DoInstead((ServerType type, string name, List<string> comps) =>
               {
                   comps.AddRange(expectedComps);
               }).Returns(NetServerEnumResults.NERR_Success)
                .MustBeCalled();
            

            target.Execute();
            var response = target.Response;
            Assert.Equal(expectedComps, response.Targets.Select(t => ((WorkgroupTarget)t).Name));
        }

        [Fact]
        public void ExecuteShouldThrowFaultExceptionWhenErrorOccurredDuringObservingTest()
        {
            WorkgroupSearchCriterion criteria = new WorkgroupSearchCriterion();
            criteria.WorkgroupName = "test";
            FindTargetsRequest request = new FindTargetsRequest();
            request.SearchCriterion = criteria;
            FindWorkgroupTargetsCommand target = new FindWorkgroupTargetsCommand(_networkBrowserMock, request);
            _networkBrowserMock
                .Arrange(browser => browser.GetNetworkComputers(ServerType.All, criteria.WorkgroupName, Arg.IsAny<List<string>>()))
                .Returns(NetServerEnumResults.ERROR_NO_BROWSER_SERVERS_FOUND)
                .MustBeCalled();
            

            Assert.Throws<InvalidOperationException>(() =>
            {
                target.Execute();
                var response = target.Response;
            });
        }
    }
}
