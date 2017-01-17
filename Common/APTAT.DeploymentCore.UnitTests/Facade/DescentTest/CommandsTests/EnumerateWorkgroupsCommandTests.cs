using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.Common;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Facade.DescentTest.CommandsTests
{
    public class EnumerateWorkgroupsCommandTests
    {
        readonly INetworkBrowser _networkBrowserMock;

        public EnumerateWorkgroupsCommandTests()
        {
            _networkBrowserMock = Mock.Create<INetworkBrowser>(Behavior.Strict);
        }

        [Fact]
        public void ExecuteTest()
        {
            List<string> expectedWorkgroups = new List<string>();
            expectedWorkgroups.Add("workgroup1");
            expectedWorkgroups.Add("workgroup2");
            expectedWorkgroups.Add("workgroup3");
            EnumerateWorkgroupsCommand target = new EnumerateWorkgroupsCommand(_networkBrowserMock);
            _networkBrowserMock
                .Arrange(browser => browser.EnumerateWorkgroups())
                .Returns(expectedWorkgroups.ToArray())
                .MustBeCalled();
            
            target.Execute();
            var response = target.Response;
            Assert.Equal(expectedWorkgroups, response.Workgroups);
        }
    }
}
