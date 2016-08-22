using DDT.Core.TelerikReports;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Telerik.Reporting;
using Telerik.Reporting.Processing;

namespace DDT.Core.Reports
{
    public interface IReportBuilder
    {
        string Build(IReportData data, ReportType reportType);
    }
    public class ReportBuilder : IReportBuilder
    {
        private const string DeploymentReportName = "DeploymentReport";
        private readonly ReportNamesCache cache = ReportNamesCache.Instance;
        private const string _reportDefinitionPath = "DDT.Core.Reports";
        private readonly IReportRenderer _reportRenderer;
        private readonly IFileHelper _fileHelper;

        public ReportBuilder(IReportRenderer reportRenderer, IFileHelper fileHelper)
        {
            _fileHelper = fileHelper;
            _reportRenderer = reportRenderer;
        }

        private string LookupForExistentReport(string operationUid, ReportType reportType)
        {
            var existentName = String.Empty;
            var reportKey = string.Format("{0}-{1}", operationUid, reportType);

            if (cache.Reports.Keys.Contains(reportKey))
            {
                existentName = cache.Reports[reportKey];
            }

            string fileName;
            if (!String.IsNullOrWhiteSpace(existentName))
            {
                if (_fileHelper.IsFileExists(existentName))
                {
                    fileName = existentName;
                    return existentName;
                }
                else
                {
                    cache.Reports.Remove(reportKey);
                }
            }
            return null;
        }
        public string Build(IReportData data, ReportType reportType)
        {
            if (data == null) return null;
            var foundReportFileName = LookupForExistentReport(data.OperationUid, reportType);
            if (!string.IsNullOrEmpty(foundReportFileName)) return foundReportFileName;

            var fileName = GetFileName(reportType);
            switch (reportType)
            {
                case ReportType.DeploymentReport:
                    cache.Reports.Add(string.Format("{0}-{1}", data.OperationUid, reportType), fileName);
                    _reportRenderer.Render(new DeploymentReportModern(), data, fileName);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("reportType");
            }

            return fileName;
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
