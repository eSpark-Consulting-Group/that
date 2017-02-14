using System;
using System.Collections.Generic;
using System.Linq;

namespace APTAT.Core.TelerikReports
{
    public sealed class ReportNamesCache : IReportNamesCache
    {
        private readonly Dictionary<string, string> _reports;

        public  ReportNamesCache()
        {
            _reports = new Dictionary<string, string>();
        }

        public string Find(string operationUid, ReportType reportType, ReportFormat reportFormat)
        {
            string existentName = null;
            string reportKey = GetReportKey(operationUid, reportType, reportFormat);

            if (_reports.Keys.Contains(reportKey))
            {
                existentName = _reports[reportKey];
            }
            return existentName;
        }

        private static string GetReportKey(string operationUid, ReportType reportType, ReportFormat reportFormat)
        {
            return string.Format("{0}-{1}-{2}", operationUid, reportType, reportFormat);
        }

        public void Remove(string operationUid, ReportType reportType, ReportFormat reportFormat)
        {            
            var key = GetReportKey(operationUid, reportType, reportFormat);
            _reports.Remove(key);
        }

        public void Add(string operationUid, ReportType reportType, ReportFormat reportFormat, string fileName)
        {
            var key = GetReportKey(operationUid, reportType, reportFormat);
            _reports.Add(key, fileName);
        }
    }
}
