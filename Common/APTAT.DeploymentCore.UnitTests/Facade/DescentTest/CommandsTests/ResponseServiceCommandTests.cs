using System;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using Telerik.JustMock;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Facade.DescentTest.CommandsTests
{
    public class ResponseServiceCommandTests
    {
        [Fact]
        public void ConstructorTest()
        {
            int request = 10;
            var target = Mock.Create<ServiceCommand<int, int>>(request); 
            Assert.Equal(request, target.Request);
            Assert.Equal(new Int32(), target.Response);
        }
    }
}
