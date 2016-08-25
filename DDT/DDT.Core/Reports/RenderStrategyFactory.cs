using System;
using System.Collections.Generic;
using System.Linq;

namespace DDT.Core.Reports
{
    public static class RenderStrategyFactory
    {
        public static IRenderStrategy GetRenderStrategy(ReportFormat reportFormat)
        {
            switch (reportFormat)
            {
                case ReportFormat.PDF:
                    return new RenderPdfStrategy();

                case ReportFormat.CSV:
                    return new RenderCsvStrategy();
                default:
                    throw new ArgumentOutOfRangeException("Not supported report format");

            }
        }
    }
}
