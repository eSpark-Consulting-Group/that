using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Facade.DescentTest.DataContractTests
{
    public class SingleIPAddressTests
    {
        [Fact]
        public void ToStringTest()
        {
            var target = new SingleIPAddress();
            target.IPAddress = "test";

            string actual = target.ToString();

            Assert.Equal(target.IPAddress, actual);
        }
    }
}
