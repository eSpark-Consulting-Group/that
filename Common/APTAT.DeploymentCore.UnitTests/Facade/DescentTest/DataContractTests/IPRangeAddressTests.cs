using System;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Facade.DescentTest.DataContractTests
{
    public class IPRangeAddressTests
    {
        [Fact]
        public void ToStringTest()
        {
            var target = new IPRangeAddress();
            target.StartAddress = "aaa";
            target.EndAddress = "bbb";
            string expected = String.Format("{0} - {1}", target.StartAddress, target.EndAddress);

            string actual = target.ToString();

            Assert.Equal(expected, actual);
        }
    }
}
