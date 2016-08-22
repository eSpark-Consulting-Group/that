using System;
using System.Net;
using APTAT.Common;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using APTAT.DeploymentCore.Transport;
using DDT.Core;
using Telerik.JustMock;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Facade.DescentTest
{
    public class IPSatretegySelectionProviderTests
    {
        private IPSatretegySelectionProvider _target;
        private readonly INetworkObserver _networkBrowserMock;

        public IPSatretegySelectionProviderTests()
        {
            _networkBrowserMock = Mock.Create<INetworkObserver>(Behavior.Strict);
            _target = new IPSatretegySelectionProvider(_networkBrowserMock);
        }

        [Fact]
        public void GetStrategyInCaseOfDnsNameAddressTest()
        {
            DnsNameAddress address = new DnsNameAddress {DnsName = "test"};
            Mock.Arrange(() => _networkBrowserMock.IsValidDnsName(address.DnsName))
                .Returns(true);
            IIPSelectionStrategy actual = _target.GetStrategy(address);

            var strategy = actual as SingleNameSelectionStrategy;
            Assert.NotNull(strategy);
            Assert.Equal(address.DnsName, strategy.HostName);
        }

        [Fact]
        public void GetStrategyInCaseOfSingleIPAddressTest()
        {
            var address = new SingleIPAddress {IPAddress = "192.168.1.1"};
            var ipAddress = IPAddress.Parse(address.IPAddress);

            var actual = _target.GetStrategy(address);

            var strategy = actual as SingleIPSelectionStrategy;
            Assert.NotNull(strategy);
            Assert.Equal(ipAddress, strategy.IP);
        }

        [Fact]
        public void GetStrategyInCaseOfSubnetMaskAddressTest()
        {
            SubnetAddress address = new SubnetAddress
            {
                IPAddress = "192.168.1.1",
                SubnetMask = "255.255.255.0"
            };
            IPAddress ipAddress = IPAddress.Parse(address.IPAddress);
            IPAddress mask = IPAddress.Parse(address.SubnetMask);

            IIPSelectionStrategy actual = _target.GetStrategy(address);

            var strategy = actual as MaskIPSelectionStrategy;
            Assert.NotNull(strategy);
            Assert.Equal(ipAddress, strategy.IP);
            Assert.Equal(mask, strategy.Mask);
        }

        [Fact]
        public void GetStrategyInCaseOfIPRangeAddressTest()
        {
            IPRangeAddress address = new IPRangeAddress();
            address.StartAddress = "192.168.1.1";
            address.EndAddress = "192.168.1.20";
            IPAddress startAddress = IPAddress.Parse(address.StartAddress);
            IPAddress finishAddress = IPAddress.Parse(address.EndAddress);

            IIPSelectionStrategy actual = _target.GetStrategy(address);

            var strategy = actual as RangeIPSelectionStrategy;
            Assert.NotNull(strategy);
            Assert.Equal(startAddress, strategy.FromIP);
            Assert.Equal(finishAddress, strategy.ToIP);
        }

        [Fact]
        public void GetStrategyShouldThrowArgumentExceptionWhenAddressTypeIsNotKnownTest()
        {
            BaseEndpointAddress addressMock = Mock.Create<BaseEndpointAddress>();

            Assert.Throws<ArgumentException>(() => _target.GetStrategy(addressMock));
        }
    }
}
