using System.Collections.Generic;
using APTAT.Core.Apt;
using APTAT.Valkyrie;

namespace APTAT.Core.Reports
{
    public class ReportData
    {
        public IResultMergeHelper MergeHelper { get; set; }
        public IAggregatedAptScanResult OperationContext { get; set; }

        public Dictionary<string, IValkyrieAnalysisResult> ValkyrieAnalysisResult { get; set; }
        public string OperationUid { get; set; }
    }
}