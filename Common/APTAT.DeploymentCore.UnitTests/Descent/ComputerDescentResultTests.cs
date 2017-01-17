using System;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;
using APTAT.DeploymentCore.Messages;
using Telerik.JustMock;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Descent
{
    public class ComputerDescentResultTests
    {
        private readonly IComputer _computer;
        private readonly Guid _computerUid;
        public ComputerDescentResultTests()
        {
            _computer = Mock.Create<IComputer>(Behavior.Strict);
            _computerUid = Guid.NewGuid();
            Mock.Arrange(() => _computer.Uid).Returns(_computerUid);
        }
        [Fact]
        public void ConstructorInitTest()
        {
            var time = DateTime.UtcNow;
            var target = new ComputerDescentResult(_computer);
            Assert.Equal(_computer, target.Computer);
            Assert.Equal(_computerUid, target.TargetUid);
            Assert.Equal(FormattedText.Empty, target.StatusMessage);
            Assert.Equal(String.Empty, target.SystemMessage);
            Assert.Equal(FormattedText.Empty, target.ShortStatusMessage);
            Assert.Equal(0, target.SystemCode);
            Assert.Equal(String.Empty, target.ErrorCode);
            Assert.False(target.Completed);
            Assert.False(target.Succeeded);
            Assert.Equal(0, target.Percentage);
            //Assert.True(target.Modified > time);
        }

        [Fact]
        public void InvalidTest()
        {
            var target = new ComputerDescentResult(_computer);
            //var time = target.Modified;
            DescentStatus status = DescentStatus.status_agent_timeout;
            string shortErrorText = "short error message";
            string detailedMessage = "detailed error message";

            target.Invalidate( status,shortErrorText,detailedMessage );

            Assert.Equal(_computer, target.Computer);
            Assert.Equal(_computerUid, target.TargetUid);
            Assert.Equal(detailedMessage, target.StatusMessage.ToString());
            Assert.Equal(detailedMessage, target.SystemMessage);
            Assert.Equal(shortErrorText, target.ShortStatusMessage.ToString());
            Assert.Equal(0, target.SystemCode);
            Assert.Equal(status.ToString(), target.ErrorCode);
            Assert.True(target.Completed);
            Assert.False(target.Succeeded);
            Assert.Equal(100, target.Percentage);
            //Assert.True(target.Modified > time);
        }

        [Fact]
        public void UpdateStatusWithOpeationResultTest()
        {
            var target = new ComputerDescentResult(_computer);
            //var time = target.Modified;
            var errorCode = DescentStatus.status_agent_timeout.ToString();
            var succeeded = true;
            string shortErrorText = "short error message";
            string detailedMessage = "detailed error message";
            string systemMessage = "system message";
            int systemCode = 1913;
            var shortErrorFormatted = new FormattedText(new ResourceText(shortErrorText, shortErrorText));
            var detailedFormatted = new FormattedText(new ResourceText(detailedMessage, detailedMessage));
            var operationResult = Mock.Create<IOperationDescentResult>(Behavior.Strict);
            Mock.Arrange(() => operationResult.ErrorCode).Returns(errorCode);
            Mock.Arrange(() => operationResult.Succeeded).Returns(succeeded);
            Mock.Arrange(() => operationResult.StatusMessage).Returns(detailedFormatted);
            Mock.Arrange(() => operationResult.ShortStatusMessage).Returns(shortErrorFormatted);
            Mock.Arrange(() => operationResult.SystemMessage).Returns(systemMessage);
            Mock.Arrange(() => operationResult.SystemCode).Returns(systemCode);
            Mock.Arrange(() => operationResult.IsCanceled).Returns(false);

            target.UpdateStatus(operationResult);
            
            Assert.Equal(_computer, target.Computer);
            Assert.Equal(_computerUid, target.TargetUid);
            Assert.Equal(detailedFormatted, target.StatusMessage);
            Assert.Equal(systemMessage, target.SystemMessage);
            Assert.Equal(shortErrorFormatted, target.ShortStatusMessage);
            Assert.Equal(systemCode, target.SystemCode);
            Assert.Equal(errorCode, target.ErrorCode);
            Assert.True(target.Completed);
            Assert.Equal(succeeded,target.Succeeded);
            Assert.Equal(100, target.Percentage);
            Assert.False(target.IsCanceled);
            //Assert.True(target.Modified > time);
        }

        [Theory]
        [InlineData(SystemKind.Windows8)]
        [InlineData(SystemKind.Windows81)]
        [InlineData(SystemKind.Windows10)]
        [InlineData(SystemKind.WindowsXP)]
        [InlineData(SystemKind.Windows7 | SystemKind.IsIntelX64Edition)]
        public void SetSystemKindTest(SystemKind systemKind)
        {
            var target = new ComputerDescentResult(_computer);
            var time = target.Modified;

            Mock.ArrangeSet(() => _computer.SystemKind = systemKind).OccursOnce();
            target.SetSystemKind(systemKind);
            Mock.AssertAll(_computer);
            //Assert.True(target.Modified > time);
        }

        [Theory]
        [InlineData("Installing...", 20)]
        [InlineData("Finishing...", 60)]
        public void UpdateStatusWithProgress(string statusMessage, int percentage)
        {
            var target = new ComputerDescentResult(_computer);
            var time = target.Modified;
            var statusMessageFormatted = new FormattedText(new ResourceText(statusMessage, statusMessage));

            var progressItem = Mock.Create<IDescentProgressItem>(Behavior.Strict);
            Mock.Arrange(() => progressItem.ProgressPercentage).Returns(percentage);
            Mock.Arrange(() => progressItem.StatusMessage).Returns(statusMessageFormatted);
            Mock.Arrange(() => progressItem.IsCanceled).Returns(false);

            target.UpdateStatus(progressItem);
            
            Assert.False(target.IsCanceled);
            Assert.Equal(_computer, target.Computer);
            Assert.Equal(_computerUid, target.TargetUid);
            Assert.Equal(statusMessageFormatted, target.StatusMessage);
            Assert.Equal(String.Empty, target.SystemMessage);
            Assert.Equal(statusMessageFormatted, target.ShortStatusMessage);
            Assert.Equal(0, target.SystemCode);
            Assert.Equal(String.Empty, target.ErrorCode);
            Assert.False(target.Completed);
            Assert.False(target.Succeeded);
            Assert.Equal(percentage, target.Percentage);
            //Assert.True(target.Modified > time);
        }
    }
}
