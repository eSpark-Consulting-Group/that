using APTAT.Core.Apt;
using APTAT.Valkyrie;
using System;
using System.Collections.Generic;

namespace APTAT.Core.Reports
{
    public interface IReportBuilder
    {
        ReportFile Build(AptReportData data, ReportType reportType, bool saveFile = true);
        AptReportData CreateReportData(IResultMergeHelper mergeHelper, IAggregatedAptScanResult operationContext,
            Dictionary<string, ValkyrieAnalysisResult> valkyrieAnalysisResult, Guid ValkyrieOperationUid, Guid FlsLookupOperationUid);
    }
}