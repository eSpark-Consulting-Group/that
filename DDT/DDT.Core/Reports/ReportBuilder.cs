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
        string Build(IReportData data, ReportType reportType, ReportFormat reportFormat);
    }
    public class ReportBuilder : IReportBuilder
    {
        private const string DeploymentReportName = "DeploymentReport";
        private readonly IReportNamesCache _cache;
        private const string _reportDefinitionPath = "DDT.Core.Reports";
        //private readonly IReportRenderer _reportRenderer;
        private readonly IFileHelper _fileHelper;

        public ReportBuilder(IFileHelper fileHelper, IReportNamesCache reportNameCache)
        {
            _fileHelper = fileHelper;
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
        public string Build(IReportData data, ReportType reportType, ReportFormat reportFormat)
        {
            if (data == null) return null;
            var foundReportFileName = LookupForExistentReport(data.OperationUid, reportType);
            if (!string.IsNullOrEmpty(foundReportFileName)) return foundReportFileName;
            var fileName = GetFileName(reportType);
            var renderer = RenderStrategyFactory.GetRenderStrategy(reportFormat);
            switch (reportType)
            {
                case ReportType.DeploymentReport:

                    _cache.Add(data.OperationUid, reportType, fileName);
                    return renderer.Render(new DeploymentReportModern(), data, fileName);
                
                default:
                    throw new ArgumentOutOfRangeException("reportType");
            }            
        }

        private static string GetFileName(ReportType reportType)
        {
            var timeStamp = DateTime.Now.ToString("s", CultureInfo.CurrentCulture).Replace(":", "").Replace("-", "");

            const string fileFormatString = "{0}-{1}";
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
