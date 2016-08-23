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
        private readonly IReportNamesCache _cache;
        private const string _reportDefinitionPath = "DDT.Core.Reports";
        private readonly IReportRenderer _reportRenderer;
        private readonly IFileHelper _fileHelper;

        public ReportBuilder(IReportRenderer reportRenderer, IFileHelper fileHelper, IReportNamesCache reportNameCache)
        {
            _fileHelper = fileHelper;
            _reportRenderer = reportRenderer;
            _cache = reportNameCache;
        }

        private string LookupForExistentReport(string operationUid, ReportType reportType)
        {
            string existentName = _cache.Find(operationUid, reportType);
            
            if (!String.IsNullOrWhiteSpace(existentName))
            {
                if (_fileHelper.IsFileExists(existentName))
                {                    
                    return existentName;
                }
                else
                {
                    _cache.Remove(operationUid, reportType);
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
                    _cache.Add(data.OperationUid, reportType, fileName);
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
