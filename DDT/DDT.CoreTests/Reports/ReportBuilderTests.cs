using Xunit;
using DDT.Core.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.JustMock;
using DDT.Core.TelerikReports;

namespace DDT.Core.Reports.Tests
{
    public class ReportBuilderTests
    {

        [Fact()]
        public void BuildTest()
        {
            IFileHelper fileHelper = new FileHelper();
            ReportBuilder builder = new ReportBuilder(new TelerikReportRenderer(), fileHelper, new ReportNamesCache());
            var newReportData = new ReportData();
            var result = builder.Build(newReportData, ReportType.DeploymentReport);
            Assert.NotNull(result);
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BuildTestGetExistentReportFromCache(bool fileExists)
        {
            var fileHelper = Mock.Create<IFileHelper>();
            Mock.Arrange(() => fileHelper.IsFileExists("")).IgnoreArguments().Returns(fileExists);

            var renderer = Mock.Create<IReportRenderer>();
            Mock.Arrange(() => renderer.Render(new DeploymentReportModern(), new ReportData(), "")).DoNothing();

            ReportBuilder builder = new ReportBuilder(renderer, fileHelper, new ReportNamesCache());

            string guid = Guid.NewGuid().ToString();
            var data1 = Mock.Create<IReportData>();
            Mock.Arrange(() => data1.OperationUid).Returns(guid);

            var data2 = Mock.Create<IReportData>();
            Mock.Arrange(() => data2.OperationUid).Returns(guid);


            var result1 = builder.Build(data1, ReportType.DeploymentReport);

            System.Threading.Thread.Sleep(2000);

            var result2 = builder.Build(data2, ReportType.DeploymentReport);
            if (fileExists)
            {
                Assert.Equal(result1, result2);
            }
            else
            {
                Assert.NotEqual(result1, result2);
            }
        }

        [Fact()]
        public void BuildReportForDifferentOperationUid()
        {
            var r = Mock.Create<IReportRenderer>();
            Mock.Arrange(() => r.Render(new DeploymentReportModern(), new ReportData(), "")).DoNothing();

            var fileHelper = Mock.Create<IFileHelper>();
            Mock.Arrange(() => fileHelper.IsFileExists("")).IgnoreArguments().Returns(true);

            ReportBuilder builder = new ReportBuilder(r, fileHelper, new ReportNamesCache());

            var data1 = Mock.Create<IReportData>();
            Mock.Arrange(() => data1.OperationUid).Returns(Guid.NewGuid().ToString());


            var data2 = Mock.Create<IReportData>();
            Mock.Arrange(() => data2.OperationUid).Returns(Guid.NewGuid().ToString());
            

            var resultA = builder.Build(data1, ReportType.DeploymentReport);
            System.Threading.Thread.Sleep(2000);
            var resultB = builder.Build(data2, ReportType.DeploymentReport);

            Assert.NotEqual(resultA.ToUpperInvariant(), resultB.ToUpperInvariant());
        }
    }
}