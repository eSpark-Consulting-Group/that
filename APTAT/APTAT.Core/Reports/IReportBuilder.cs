using APTAT.Core.Apt;
using System;
using System.Collections.Generic;

namespace APTAT.Core.Reports
{
    public interface IReportBuilder
    {
        ReportFile Build(AptReportData data, ReportType reportType, bool saveFile = true);
        AptReportData CreateReportData(IResultMergeHelper mergeHelper, IAggregatedAptScanResult operationContext,
            Dictionary<string, FileResult> valkyrieAnalysisResult, Guid ValkyrieOperationUid, Guid FlsLookupOperationUid);
    }
}