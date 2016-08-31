using Xunit;
using DDT.Core.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDT.Core.Reports.Tests
{
    public class RenderStrategyFactoryTests
    {
        [Theory]
        [InlineData(ReportFormat.PDF)]
        [InlineData(ReportFormat.CSV)]
        public void GetRenderStrategyTest(ReportFormat reportFormat)
        {
            var ttt = RenderStrategyFactory.GetRenderStrategy(reportFormat);
            Assert.NotNull(ttt);
        }
    }
}
