using System.Collections.Generic;

namespace APTAT.Core.Reports
{
    public partial class AptReportData:  IAptReportData
    {
        public class MergedScanResults<TDetailedBadFile> where TDetailedBadFile: DetailedBadFile
        {
            public IEnumerable<ScannedTarget> MergedTargets { get; set; }
            public IEnumerable<TDetailedBadFile> MergeUniqueResults { get; set; }
            public IEnumerable<TDetailedBadFile> MergeNonUniqueResults { get; set; }
        }

    }
}
