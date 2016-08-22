using DDT.Core.TelerikReports;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Telerik.Reporting;
using Telerik.Reporting.Processing;

namespace DDT.Core.Reports
{

    public class ReportBuilder
    {
        private const string DeploymentReportName = "DeploymentReport";
        private readonly ReportNamesCache cache = ReportNamesCache.Instance;        
        private const string _reportDefinitionPath = "DDT.Core.Reports";


        public string Build(ReportData data, ReportType reportType)
        {
            
            if (data == null)
                return null;

            var existentName = String.Empty;
            var flsReportKey = string.Format("{0}-{1}", data.OperationUid, reportType);
            var reportKey = String.Empty;
            if (cache.Reports.Keys.Contains(flsReportKey))
            {
                existentName = cache.Reports[flsReportKey];
                reportKey = flsReportKey;
            }
            string fileName;
            if (!String.IsNullOrWhiteSpace(existentName))
            {
                if (File.Exists(existentName))
                {
                    fileName = existentName;
                    return existentName;
                }
                else
                {
                    cache.Reports.Remove(reportKey);
                }
            }

            fileName = GetFileName(reportType);
            switch (reportType)
            {
                case ReportType.DeploymentReport:
                    cache.Reports.Add(string.Format("{0}-{1}", data.OperationUid, reportType), fileName);
                    BuildDeploymentReportModern(data, fileName);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("reportType");
            }

            return fileName;
        }

        public ReportSource BuildDeploymentReport(ReportData data)
        {
            InstanceReportSource reportSource = new InstanceReportSource();
            DeploymentReportModern report = new DeploymentReportModern();
            report.DataSource = data;
            reportSource.ReportDocument = report;
            return reportSource;        
        }

        private void BuildDeploymentReportModern(ReportData data, string fileName)
        {
            var reportSource = BuildDeploymentReport(data);

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

        private static string GetFileName(ReportType reportType)
        {
            var timeStamp = DateTime.Now.ToString("s", CultureInfo.CurrentCulture).Replace(":", "").Replace("-", "");

            const string fileFormatString = "{0}-{1}.pdf";
            switch (reportType)
            {
                case ReportType.DeploymentReport:
                    return String.Format(fileFormatString, DeploymentReportName, timeStamp);

                default:
                    throw new ArgumentOutOfRangeException("reportType");
            }
        }

    }
}
