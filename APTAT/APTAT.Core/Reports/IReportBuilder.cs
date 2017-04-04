using APTAT.Core.Apt;
using APTAT.Valkyrie;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APTAT.Core.Reports
{
    public interface IReportBuilder
    {
        Task<ReportFile> BuildReportAsync(ReportData reportData, ReportType reportType, bool saveFile = true);        
    }
}