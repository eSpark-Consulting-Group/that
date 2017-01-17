using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Messages;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Descent
{
    public class OperationDescentResultTests
    {
        [Theory]
        [InlineData(true, "", "", "", "", 0, false)]
        [InlineData(false, "Login Problem full", "Login Problem short", "4133412", "Login problem system Message", 1232131, false)]
        [InlineData(false, "Operation was canceled full", "Operation was canceled short", "12341", "Operation was canceled system", -1, true)]
        public void ConstructorTest(bool succeeded, string statusMessage, string shortStatusMessage,
            string errorCode, string systemMessage, int systemCode,
            bool isCanceled)
        {
            var statusMessageFormatted = new FormattedText(new ResourceText(statusMessage, statusMessage));
            var shortStatusMessageFormatted = new FormattedText(new ResourceText(shortStatusMessage, shortStatusMessage));
            var target = new OperationDescentResult(succeeded, statusMessageFormatted, shortStatusMessageFormatted, errorCode,
                systemMessage, systemCode, isCanceled);

            Assert.Equal(succeeded, target.Succeeded);
            Assert.Equal(statusMessage, target.StatusMessage.ToString());
            Assert.Equal(shortStatusMessage, target.ShortStatusMessage.ToString());
            Assert.Equal(errorCode, target.ErrorCode);
            Assert.Equal(systemMessage, target.SystemMessage);
            Assert.Equal(systemCode, target.SystemCode);
            Assert.Equal(isCanceled, target.IsCanceled);
        }
    }
}
