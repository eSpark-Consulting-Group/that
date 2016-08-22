using System;
using System.Reflection;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using Telerik.JustMock;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Facade.DescentTest.CommandsTests
{
    public class RequestServiceCommandTests
    {
        [Fact]
        public void ConstructorTest()
        {
            int request = 10;

            var target = Mock.Create<ServiceCommand<int, int>>(request);
            
            Assert.Equal(request, target.Request);
        }

        [Fact]
        public void ConstructorShouldThrowArgumentNullExceptionWhenRequestIsNullTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Mock.Create<RequestServiceCommand<object>>((object) null);
            });
        }
    }
}
