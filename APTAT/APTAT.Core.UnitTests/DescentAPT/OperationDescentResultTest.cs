using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.JustMock;
using Xunit;

namespace APTAT.Core.UnitTests.DescentAPT
{
    public class OperationDescentResultTest
    {
        [Fact(DisplayName = "OperationDescentResult constructor test.")]
        public void OperationDescentResultConstructorTest()
        {
            bool succeded = true;
            FormattedText statusMessage = Arg.IsAny<FormattedText>();
            FormattedText shortStatusMessage = Arg.IsAny<FormattedText>();
            string errorCode = "Error12";
            string systemMessage = "SysMessage";
            int systemCode = 57;
            bool isCancelled = false;

            OperationDescentResult result =
                new OperationDescentResult(succeded, statusMessage,
                shortStatusMessage, errorCode, systemMessage,
                systemCode, isCancelled);

            Assert.Equal(succeded, result.Succeeded);
            Assert.Equal(statusMessage, result.StatusMessage);
            Assert.Equal(shortStatusMessage, result.ShortStatusMessage);
            Assert.Equal(errorCode, result.ErrorCode);
            Assert.Equal(systemMessage, result.SystemMessage);
            Assert.Equal(systemCode, result.SystemCode);
            Assert.Equal(isCancelled, result.IsCanceled);

        }
    }
}
