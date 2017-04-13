using System.Collections.Generic;
using APTAT.Core.Apt;
using APTAT.Valkyrie;

namespace APTAT.Core.Reports
{
    public interface IReportData
    {
        IResultMergeHelper MergeHelper { get; set; }
        IAggregatedAptScanResult OperationContext { get; set; }
        string OperationUid { get; set; }
        Dictionary<string, IValkyrieAnalysisResult> ValkyrieAnalysisResult { get; set; }
    }
}