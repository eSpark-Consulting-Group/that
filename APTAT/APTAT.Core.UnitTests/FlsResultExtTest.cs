using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Messages;
using Telerik.JustMock;
using Xunit;
using APTAT.Common;
using System.Diagnostics;
using System.Globalization;
using System.Net.Mime;
using APTAT.Common.MachineKey;
using APTAT.Core.CMC;
using APTAT.Core.CMC.Request;
using Telerik.JustMock.Helpers;

namespace APTAT.Core.UnitTests
{
    public class FlsResultExtTest
    {
        [Theory(DisplayName = "Test method IsNotAnalyzed for analysed files.")]
        [InlineData(Descent.Apt.CloudVerdict.FailedToAnalyze)]
        [InlineData(Descent.Apt.CloudVerdict.NoVerdict)]
        public void TestExtensionMethodIsNotAnalyzedForAnalysed(Descent.Apt.CloudVerdict verdict)
        {
            //Arrange
            IFlsResult testdata = Mock.Create<IFlsResult>(Behavior.Strict);
            Mock.Arrange(() => testdata.Cloud).Returns(verdict);
            //Act
            var result = FlsResultExt.IsNotAnalyzed(testdata);
            //Assert
            Assert.True(result);
        }
        [Theory(DisplayName = "Test method IsNotAnalyzed for not analysed files.")]
        [InlineData(Descent.Apt.CloudVerdict.FirstSeen)]
        [InlineData(Descent.Apt.CloudVerdict.Malware)]
        [InlineData(Descent.Apt.CloudVerdict.Safe)]
        [InlineData(Descent.Apt.CloudVerdict.Unknown)]
        public void TestExtensionMethodIsNotAnalyzedForNotAnalysed(Descent.Apt.CloudVerdict verdict)
        {
            //Arrange
            IFlsResult testdata = Mock.Create<IFlsResult>(Behavior.Strict);
            Mock.Arrange(() => testdata.Cloud).Returns(verdict);
            //Act
            var result = FlsResultExt.IsNotAnalyzed(testdata);
            //Assert
            Assert.False(result);
        }
        [Theory(DisplayName = "Test method IsUnknown for unknown files.")]
        [InlineData(Descent.Apt.CloudVerdict.FirstSeen)]
        [InlineData(Descent.Apt.CloudVerdict.Unknown)]
        public void TestExtensionMethodIsUnknownForNotAnalysed(Descent.Apt.CloudVerdict verdict)
        {
            //Arrange
            IFlsResult testdata = Mock.Create<IFlsResult>(Behavior.Strict);

            Mock.Arrange(() => testdata.Cloud).Returns(verdict);
            //Act
            var result = FlsResultExt.IsUnknown(testdata);
            //Assert
            Assert.True(result);
        }
        [Theory(DisplayName = "Test method IsUnknown for known files.")]
        [InlineData(Descent.Apt.CloudVerdict.FailedToAnalyze)]
        [InlineData(Descent.Apt.CloudVerdict.NoVerdict)]
        [InlineData(Descent.Apt.CloudVerdict.Malware)]
        [InlineData(Descent.Apt.CloudVerdict.Safe)]
        public void TestExtensionMethodIsUnknownForAnalysed(Descent.Apt.CloudVerdict verdict)
        {
            //Arrange
            IFlsResult testdata = Mock.Create<IFlsResult>(Behavior.Strict);
            Mock.Arrange(() => testdata.Cloud).Returns(verdict);
            //Act
            var result = FlsResultExt.IsUnknown(testdata);
            //Assert
            Assert.False(result);
        }
    }
}
