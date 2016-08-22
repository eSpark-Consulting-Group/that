using DDT.Core.TelerikReports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Reporting;
using Telerik.Reporting.Processing;

namespace DDT.Core.Reports
{
    public interface IReportRenderer
    {
        void Render(Telerik.Reporting.Report report, object data, string fileName);
    }

    public class TelerikReportRenderer : IReportRenderer
    {
        public void Render(Telerik.Reporting.Report report, object data, string fileName)
        {
            InstanceReportSource reportSource = new InstanceReportSource();          
            report.DataSource = data;
            reportSource.ReportDocument = report;
            RenderReportToPdf(fileName, reportSource);
        }

        private static void RenderReportToPdf(string fileName, ReportSource reportSource)
        {
            ReportProcessor reportProcessor = new ReportProcessor();
            RenderingResult renderResult = reportProcessor.RenderReport("PDF", reportSource, new System.Collections.Hashtable());

            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                fs.Write(renderResult.DocumentBytes, 0, renderResult.DocumentBytes.Length);
            }
        }
    }
}
