using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Facade.DescentTest.DataContractTests
{
    public class DnsNameAddressTests
    {
        [Fact]
        public void ToStringTest()
        {
            DnsNameAddress target = new DnsNameAddress();
            target.DnsName = "test";

            string actual = target.ToString();

            Assert.Equal(target.DnsName, actual);
        }
    }
}
