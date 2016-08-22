using System;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Facade.DescentTest.DataContractTests
{
    public class SubnetMaskAddressTests
    {
        [Fact]
        public void ToStringTest()
        {
            var target = new SubnetAddress();
            target.IPAddress = "address";
            target.SubnetMask = "mask";
            string expected = String.Format("{0} / {1}", target.IPAddress, target.SubnetMask);

            string actual = target.ToString();

            Assert.Equal(expected, actual);
        }
    }
}
