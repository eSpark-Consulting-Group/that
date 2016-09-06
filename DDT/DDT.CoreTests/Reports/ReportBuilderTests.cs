using Xunit;
using DDT.Core.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.JustMock;
using DDT.Core.TelerikReports;
using System.IO;

namespace DDT.Core.Reports.Tests
{
    public class ReportBuilderTests
    {

        [Fact()]
        public void BuildTest()
        {
            IFileHelper fileHelper = new FileHelper();
            ReportBuilder builder = new ReportBuilder(fileHelper, new ReportNamesCache());
            var newReportData = new ReportData();
            var result = builder.Build(newReportData, ReportType.DeploymentReport, ReportFormat.CSV);
            Assert.NotNull(result);
            var ext = Path.GetExtension(result);         
            Assert.Equal(".csv", ext);
        }

        [Fact]
        public void NullArgTest()
        {
            IFileHelper fileHelper = new FileHelper();
            ReportBuilder builder = new ReportBuilder(fileHelper, new ReportNamesCache());
            var result = builder.Build(null, ReportType.DeploymentReport, ReportFormat.PDF);
            Assert.Null(result);
        }

        [Fact]
        public void ReportBuildExceptionThrownTest()
        {
            IFileHelper fileHelper = new FileHelper();
            ReportBuilder builder = new ReportBuilder(fileHelper, new ReportNamesCache());

            var maxV = Enum.GetValues(typeof(ReportType)).Cast<int>().Max();
            maxV++;

            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    var result = builder.Build(new ReportData(), (ReportType) maxV,ReportFormat.PDF);
                }
                );
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BuildTestGetExistentReportFromCache(bool fileExists)
        {
            var fileHelper = Mock.Create<IFileHelper>();
            Mock.Arrange(() => fileHelper.IsFileExists("")).IgnoreArguments().Returns(fileExists);
            
            ReportBuilder builder = new ReportBuilder(fileHelper, new ReportNamesCache());

            string guid = Guid.NewGuid().ToString();
            var data1 = Mock.Create<IReportData>();
            Mock.Arrange(() => data1.OperationUid).Returns(guid);

            var data2 = Mock.Create<IReportData>();
            Mock.Arrange(() => data2.OperationUid).Returns(guid);
            
            var result1 = builder.Build(data1, ReportType.DeploymentReport, ReportFormat.PDF);

            System.Threading.Thread.Sleep(2000);

            var result2 = builder.Build(data2, ReportType.DeploymentReport, ReportFormat.PDF);
            if (fileExists)
            {
                Assert.Equal(result1, result2);
            }
            else
            {
                Assert.NotEqual(result1, result2);
            }
        }


        [Fact]
        public void BuildTestGetExistentReportFromCache2()
        {
            var fileHelper = Mock.Create<IFileHelper>();
            Mock.Arrange(() => fileHelper.IsFileExists("")).IgnoreArguments().Returns(true);            

            ReportBuilder builder = new ReportBuilder(fileHelper, new ReportNamesCache());

            string guid = Guid.NewGuid().ToString();
            var data1 = Mock.Create<IReportData>();
            Mock.Arrange(() => data1.OperationUid).Returns(guid);

            var data2 = Mock.Create<IReportData>();
            Mock.Arrange(() => data2.OperationUid).Returns(guid);

            var result1 = builder.Build(data1, ReportType.DeploymentReport, ReportFormat.PDF);

            System.Threading.Thread.Sleep(2000);

            var result2 = builder.Build(data2, ReportType.DeploymentReport, ReportFormat.CSV);
            
            Assert.NotEqual(result1, result2);

            var result3 = builder.Build(data1, ReportType.DeploymentReport, ReportFormat.PDF);
            Assert.Equal(result1, result3);

            var result4 = builder.Build(data1, ReportType.DeploymentReport, ReportFormat.CSV);
            Assert.Equal(result2, result4);
        }


        [Fact()]
        public void BuildReportForDifferentOperationUid()
        {
            var fileHelper = Mock.Create<IFileHelper>();
            Mock.Arrange(() => fileHelper.IsFileExists("")).IgnoreArguments().Returns(true);

            ReportBuilder builder = new ReportBuilder(fileHelper, new ReportNamesCache());
            var data1 = Mock.Create<IReportData>();
            Mock.Arrange(() => data1.OperationUid).Returns(Guid.NewGuid().ToString());
            var data2 = Mock.Create<IReportData>();
            Mock.Arrange(() => data2.OperationUid).Returns(Guid.NewGuid().ToString());

            var resultA = builder.Build(data1, ReportType.DeploymentReport, ReportFormat.PDF);
            System.Threading.Thread.Sleep(2000);
            var resultB = builder.Build(data2, ReportType.DeploymentReport, ReportFormat.PDF);

            Assert.NotEqual(resultA.ToUpperInvariant(), resultB.ToUpperInvariant());
        }
    }
}