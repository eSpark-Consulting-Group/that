using System.IO;
using Telerik.Reporting;
using Telerik.Reporting.Processing;

namespace APTAT.Core.TelerikReports
{
    public class RenderPdfStrategy : IRenderStrategy
    {
        public byte[] Render(Telerik.Reporting.Report report, object data)
        {
            
            InstanceReportSource reportSource = new InstanceReportSource();
            report.DataSource = data;
            reportSource.ReportDocument = report;

            ReportProcessor reportProcessor = new ReportProcessor();
            RenderingResult renderResult = reportProcessor.RenderReport("PDF", reportSource, new System.Collections.Hashtable());

            return renderResult.DocumentBytes;
        }


    }
}
