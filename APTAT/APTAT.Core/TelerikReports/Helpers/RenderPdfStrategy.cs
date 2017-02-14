using System.IO;
using Telerik.Reporting;
using Telerik.Reporting.Processing;

namespace APTAT.Core.TelerikReports
{
    public class RenderPdfStrategy : IRenderStrategy
    {
        public  string Render(Telerik.Reporting.Report report, object data, string fileName)
        {
            var path = fileName + ".pdf";
            InstanceReportSource reportSource = new InstanceReportSource();
            report.DataSource = data;
            reportSource.ReportDocument = report;

            ReportProcessor reportProcessor = new ReportProcessor();
            RenderingResult renderResult = reportProcessor.RenderReport("PDF", reportSource, new System.Collections.Hashtable());

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                fs.Write(renderResult.DocumentBytes, 0, renderResult.DocumentBytes.Length);
            }
            return path;
        }
    }
}
