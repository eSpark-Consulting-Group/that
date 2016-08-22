
using System;
using System.Linq;
using System.Net;
using APTAT.Common;
using APTAT.DeploymentCore.Facade;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using APTAT.DeploymentCore.Facade.Descent.MessageContracts;
using APTAT.DeploymentCore.Transport;
using DDT.Core;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Facade.DescentTest.CommandsTests
{
    public class FindEndpointTargetsCommandTests //: BaseServiceTest<DescentService>
    {

        public FindEndpointTargetsCommandTests()
        {
        }

        [Fact]
        public void ConstructorTest()
        {
            EndpointAddressCollection addresses = new EndpointAddressCollection();
            addresses.Add(new DnsNameAddress { DnsName = "test" });
            FindTargetsRequest request = new FindTargetsRequest
            {
                SearchCriterion = new EndpointSearchCriterion
                {
                    EndpointAddresses = addresses
                }
            };

            var ipStrategyProvider = Mock.Create<IIPSatretegySelectionProvider>(Behavior.Strict);
            FindEndpointTargetsCommand target = new FindEndpointTargetsCommand(ipStrategyProvider,request);

            Assert.Equal(request, target.Request);
        }

        [Fact]
        public void ExecuteForSingleIpAddressTest()
        {
            var request = new FindTargetsRequest();
            var searchCriteria = new EndpointSearchCriterion
            {
                EndpointAddresses = new EndpointAddressCollection()
            };
            var ipAddress = "192.168.71.15";
            searchCriteria.EndpointAddresses.Add(new SingleIPAddress
            {
                IPAddress = ipAddress,
            });
            request.SearchCriterion = searchCriteria;
            var ipStrategyProvider = Mock.Create<IIPSatretegySelectionProvider>(Behavior.Strict);
            var targetCommand = new FindEndpointTargetsCommand(ipStrategyProvider,request);

            targetCommand.Execute();

            var response = targetCommand.Response;
            Assert.True(response.IsCompleted);
            Assert.Equal(1, response.Targets.Count);
            var target = response.Targets.First();
            Assert.Equal(ipAddress, target.IPAddress);
            Assert.NotEqual(Guid.Empty, target.Uid);
            Assert.Equal(ipAddress, target.Name);
            Assert.Null(target.SystemKind);
            Assert.Null(target.LdapPath);
            Assert.Null(target.OrganizationUnit);
            Assert.Equal(DeploymentTargetSource.Ip,target.Source);
            Assert.False(target.IsOnline);
            Assert.False(target.IsAptHost);
        }

        [Fact]
        public void ExecuteForRangeOfIpAddressTest()
        {
            var request = new FindTargetsRequest();
            var searchCriteria = new EndpointSearchCriterion
            {
                EndpointAddresses = new EndpointAddressCollection()
            };
            var ipAddress = new string[] { "192.168.71.2", "192.168.71.15" };
            searchCriteria.EndpointAddresses.Add(new IPRangeAddress()
            {
                StartAddress = ipAddress[0],
                EndAddress = ipAddress[1]
            });

            request.SearchCriterion = searchCriteria;
            var strategy = Mock.Create<IIPSelectionStrategy>(Behavior.Strict);
            var provider = Mock.Create<IIPSatretegySelectionProvider>(Behavior.Strict);
            Mock.Arrange(() => provider.GetStrategy(Arg.IsAny<IPRangeAddress>()))
                .Returns(strategy);
            Mock.Arrange(() => strategy.GetIPs()).Returns(ipAddress.Select(c => IPAddress.Parse(c)));
            var targetCommand = new FindEndpointTargetsCommand(provider, request);

            targetCommand.Execute();

            var response = targetCommand.Response;
            Assert.True(response.IsCompleted);
            Assert.Equal(2, response.Targets.Count);
            Assert.Equal(ipAddress, response.Targets.Select(t => t.Name));
            Assert.Equal(ipAddress, response.Targets.Select(t => t.IPAddress));
            foreach (var target in response.Targets)
            {
                Assert.NotEqual(Guid.Empty, target.Uid);
                Assert.Null(target.SystemKind);
                Assert.Null(target.LdapPath);
                Assert.Null(target.OrganizationUnit);
                Assert.Equal(DeploymentTargetSource.Ip, target.Source);
                Assert.False(target.IsOnline);
                Assert.False(target.IsAptHost);
            }
        }
    }
}