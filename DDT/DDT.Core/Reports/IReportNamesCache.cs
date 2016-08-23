using System;
using System.Collections.Generic;
using System.Linq;

namespace DDT.Core.Reports
{
    public interface IReportNamesCache
    {
        string Find(string operationUid, ReportType reportType);
        void Remove(string operationUid, ReportType reportType);
        void Add(string operationUid, ReportType reportType, string fileName);
    }
}
