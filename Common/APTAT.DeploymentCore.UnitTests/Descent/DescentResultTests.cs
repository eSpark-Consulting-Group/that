using System;
using System.Collections.Generic;

using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Messages;
using Telerik.JustMock;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Descent
{
    public class DescentResultTests
    {

        private static readonly Guid Comp1Uid = new Guid("3EC5EB9F-84A9-4A53-9381-9CE1CB15953D");
        private static readonly Guid Comp2Uid = new Guid("8A0CF07D-8994-440B-980F-175938956597");
        private static readonly Guid Comp3Uid = new Guid("C08ABED7-21E8-4AE3-B816-1485D1277D0D");

        private static readonly IComputer Computer1 = Mock.Create<IComputer>(Behavior.Strict);
        private static readonly IComputer Computer2 = Mock.Create<IComputer>(Behavior.Strict);
        private static readonly IComputer Computer3 = Mock.Create<IComputer>(Behavior.Strict);

        private static readonly List<IComputer> Targets = new List<IComputer> {Computer1, Computer2, Computer3};
        private readonly IDevicesSourceDescription _deviceSourceDescription;

        static DescentResultTests()
        {
            Mock.Arrange(() => Computer1.Uid).Returns(Comp1Uid);
            Mock.Arrange(() => Computer2.Uid).Returns(Comp2Uid);
            Mock.Arrange(() => Computer3.Uid).Returns(Comp3Uid);
            Mock.Arrange(() => Computer1.Name).Returns("comp1");
            Mock.Arrange(() => Computer2.Name).Returns("comp2");
            Mock.Arrange(() => Computer3.Name).Returns("comp3");
        }

        public DescentResultTests()
        {
            _deviceSourceDescription = Mock.Create<IDevicesSourceDescription>(Behavior.Strict);
        }

        [Fact]
        public void ConstructorTest()
        {
            var target = new DescentResult(Targets, _deviceSourceDescription);
            Assert.Equal(Targets.Count, target.TotalTargetsCount);
            Assert.NotEqual(Guid.Empty, target.OperationUid);
            Assert.Equal(_deviceSourceDescription, target.DevicesSourceDescription);
            Assert.Null(target.CompletedTime);
            Assert.False(target.Completed);
            Assert.Equal(3,target.TotalTargetsCount);
            Assert.Equal(3, target.ComputerDescentResults.Count);
        }

        [Theory]
        [MemberData(nameof(GetOperationResultsEnumerable))]
        public void Test(bool ok, Guid computerUid, string errorMessage, string shortStatusMessage, string errorCode, int systemCode, string systemMessage)
        {
            var target = new DescentResult(Targets, _deviceSourceDescription);
            var errorCodeFormatted = new FormattedText(new ResourceText(errorCode, errorCode));
            var shortStatusFormatted = new FormattedText(new ResourceText(shortStatusMessage, shortStatusMessage));
            target.OnDescendOperationEvent(ok, computerUid, errorCodeFormatted, shortStatusFormatted, errorCode,
                systemCode, systemMessage);
            Assert.Null(target.CompletedTime);
            Assert.False(target.Completed);
            Assert.Equal(3, target.TotalTargetsCount);
            Assert.Equal(3, target.ComputerDescentResults.Count);
            var computerResult = target.GetComputerDescentResultByTargetUid(computerUid);
            Assert.True(computerResult.Completed);
            Assert.Equal(ok,computerResult.Succeeded);
        }
        public static IEnumerable<object[]> GetOperationResultsEnumerable()
        {
            yield return new object[] { true, Computer1.Uid, "", "", "", 3, ""  };
            yield return new object[] { true, Computer2.Uid, "", "", "", 3, "" };
            yield return new object[] { true, Computer3.Uid, "", "", "", 3, "" };
        }
    }
}
