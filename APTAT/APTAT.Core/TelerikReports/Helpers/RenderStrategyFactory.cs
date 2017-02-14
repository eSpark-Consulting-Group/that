using System;

namespace APTAT.Core.TelerikReports
{
    public static class RenderStrategyFactory
    {
        public static IRenderStrategy GetRenderStrategy(ReportFormat reportFormat)
        {
            switch (reportFormat)
            {
                case ReportFormat.PDF:
                    return new RenderPdfStrategy();         

                default:
                    throw new ArgumentOutOfRangeException("Not supported report format");

            }
        }
    }
}
