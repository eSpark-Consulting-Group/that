using APTAT.Core.Reports;
using System;
using System.Collections.Generic;
using System.Linq;

namespace APTAT.Core.TelerikReports
{
    public interface IReportNamesCache
    {
        string Find(string operationUid, ReportType reportType, ReportFormat reportFormat);
        void Remove(string operationUid, ReportType reportType, ReportFormat reportFormat);
        void Add(string operationUid, ReportType reportType, ReportFormat reportFormat, string fileName);
    }
}
