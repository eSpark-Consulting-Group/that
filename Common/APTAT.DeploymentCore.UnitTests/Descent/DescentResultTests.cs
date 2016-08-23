using System;
using System.Collections.Generic;

using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Messages;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Descent
{
    public class DescentResultTests
    {

        private static readonly Guid Comp1Uid = new Guid("3EC5EB9F-84A9-4A53-9381-9CE1CB15953D");
        private static readonly Guid Comp2Uid = new Guid("8A0CF07D-8994-440B-980F-175938956597");
        private static readonly Guid Comp3Uid = new Guid("C08ABED7-21E8-4AE3-B816-1485D1277D0D");
        private static readonly Guid UknownCompUid = new Guid("C0813ED7-21E8-4AE3-B816-1485D1277D0D");

        private static readonly IComputer Computer1 = Mock.Create<IComputer>(Behavior.Strict);
        private static readonly IComputer Computer2 = Mock.Create<IComputer>(Behavior.Strict);
        private static readonly IComputer Computer3 = Mock.Create<IComputer>(Behavior.Strict);

        private static readonly List<IComputer> Targets = new List<IComputer> {Computer1, Computer2, Computer3};
        private readonly IDevicesSourceDescription _deviceSourceDescription;

        private static readonly DescentResult.IComputerDescentResultFactory _computerDescentResultFactory;

        static DescentResultTests()
        {
            Mock.Arrange(() => Computer1.Uid).Returns(Comp1Uid);
            Mock.Arrange(() => Computer2.Uid).Returns(Comp2Uid);
            Mock.Arrange(() => Computer3.Uid).Returns(Comp3Uid);
            Mock.Arrange(() => Computer1.Name).Returns("comp1");
            Mock.Arrange(() => Computer2.Name).Returns("comp2");
            Mock.Arrange(() => Computer3.Name).Returns("comp3");
            _computerDescentResultFactory = Mock.Create<DescentResult.IComputerDescentResultFactory>(Behavior.Strict);
            Mock.Arrange(() => _computerDescentResultFactory.CreateComputerDescentResult(Arg.IsAny<IComputer>()))
                .Returns((IComputer c) =>
                {
                    var r = Mock.Create<IComputerDescentResultInternal>(Behavior.Strict);
                    var modified = DateTime.UtcNow;
                    var completed = false;
                    var succeeded = false;
                    Mock.Arrange(() => r.UpdateStatus(Arg.IsAny<IOperationDescentResult>()))
                        .DoInstead((IOperationDescentResult result) =>
                        {
                            modified = DateTime.UtcNow;
                            completed = true;
                            succeeded = result.Succeeded;
                        }).MustBeCalled();
                    Mock.Arrange(() => r.Succeeded).Returns(() => succeeded);
                    Mock.Arrange(() => r.Completed).Returns(() => completed);
                    Mock.Arrange(() => r.Modified).Returns(() => modified);
                    Mock.Arrange(() => r.Computer).Returns(c);
                    return r;
                });
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
            Assert.Equal(0, target.FailedCount);
            Assert.Equal(0, target.SucceededCount);
            Assert.Equal(3, target.InProgressCount);
            Assert.Equal(3,target.TotalTargetsCount);
            Assert.Equal(3, target.ComputerDescentResults.Count);
        }

        [Fact]
        public void OnDescentOperationEventTest()
        {
            
            var target = new DescentResult(Targets, _deviceSourceDescription, _computerDescentResultFactory);
            var errorCodeFormatted = new FormattedText(new ResourceText("", ""));
            var shortStatusFormatted = new FormattedText(new ResourceText("", ""));
            target.OnDescendOperationEvent(true, Computer1.Uid, errorCodeFormatted, shortStatusFormatted, "", 3, "");
            Assert.Null(target.CompletedTime);
            Assert.False(target.Completed);
            Assert.Equal(3, target.TotalTargetsCount);
            Assert.Equal(0, target.FailedCount);
            Assert.Equal(1, target.SucceededCount);
            Assert.Equal(2, target.InProgressCount);
            Assert.Equal(3, target.ComputerDescentResults.Count);
            target.OnDescendOperationEvent(true, Computer2.Uid, errorCodeFormatted, shortStatusFormatted, "", 3, "");
            Assert.Null(target.CompletedTime);
            Assert.False(target.Completed);
            Assert.Equal(2, target.SucceededCount);
            Assert.Equal(0, target.FailedCount);
            Assert.Equal(1, target.InProgressCount);
            Assert.Equal(3, target.TotalTargetsCount);
            Assert.Equal(3, target.ComputerDescentResults.Count);
            target.OnDescendOperationEvent(false, Computer3.Uid, errorCodeFormatted, shortStatusFormatted, "", 3, "");
            Assert.NotNull(target.CompletedTime);
            Assert.True(target.Completed);
            Assert.Equal(2, target.SucceededCount);
            Assert.Equal(1, target.FailedCount);
            Assert.Equal(0, target.InProgressCount);
            Assert.Equal(3, target.TotalTargetsCount);
            Assert.Equal(3, target.ComputerDescentResults.Count);
        }

        [Fact]
        public void OnDescentOperationEventWithUknownCompUidTest()
        {
            var target = new DescentResult(Targets, _deviceSourceDescription, _computerDescentResultFactory);
            var errorCodeFormatted = new FormattedText(new ResourceText("", ""));
            var shortStatusFormatted = new FormattedText(new ResourceText("", ""));
            
            var ex = Assert.Throws<InvalidOperationException>(
                () => target.OnDescendOperationEvent(true, UknownCompUid, errorCodeFormatted, shortStatusFormatted,"", 3, ""));
            Assert.Equal("Computer result not found by UID " + UknownCompUid, ex.Message);
        }

        public static IEnumerable<object[]> GetOperationResultsEnumerable()
        {
            yield return new object[] { true, Computer1.Uid, "", "", "", 3, ""  };
            yield return new object[] { true, Computer2.Uid, "", "", "", 3, "" };
            yield return new object[] { true, Computer3.Uid, "", "", "", 3, "" };
        }
    }
}
