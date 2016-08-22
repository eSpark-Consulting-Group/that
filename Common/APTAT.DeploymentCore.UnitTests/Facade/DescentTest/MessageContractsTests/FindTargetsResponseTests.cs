using APTAT.DeploymentCore.Facade.Descent.MessageContracts;

using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Facade.DescentTest.MessageContractsTests
{
    public class FindTargetsResponseTests
    {
        [Fact]
        public void ConstructorTestTest()
        {
            FindTargetsResponse target = new FindTargetsResponse();
            Assert.NotNull(target.Targets);
            Assert.False(target.IsCompleted);
            Assert.True(target.Targets.Count == 0);
        }

    }
}
