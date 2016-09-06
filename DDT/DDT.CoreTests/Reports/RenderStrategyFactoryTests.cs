using Xunit;

namespace DDT.Core.Reports.Tests
{
    public class RenderStrategyFactoryTests
    {
        [Theory]
        [InlineData(ReportFormat.PDF)]
        [InlineData(ReportFormat.CSV)]
        public void GetRenderStrategyTest(ReportFormat reportFormat)
        {
            var strategy = RenderStrategyFactory.GetRenderStrategy(reportFormat);
            Assert.NotNull(strategy);

            if (reportFormat == ReportFormat.PDF)
            {
                Assert.IsType(typeof(RenderPdfStrategy), strategy);
            }
            else if (reportFormat == ReportFormat.CSV)
            {
                Assert.IsType(typeof(RenderCsvStrategy), strategy);
            }
        }
    }
}
