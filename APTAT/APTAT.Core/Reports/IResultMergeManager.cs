using System.Collections.Generic;
using APTAT.Core.Descent.Apt;
using APTAT.Valkyrie;

namespace APTAT.Core.Reports
{
    public interface IResultMergeHelper
    {
        FilesStatistics MergeResults(IEnumerable<IAptScanResult> flsResults, Dictionary<string, ValkyrieAnalysisResult> valkyrieResults, out IEnumerable<ScannedTarget> mergedTargets, out IEnumerable<DetailedBadFile> uniqueBadFiles, out IEnumerable<DetailedBadFile> nonUniqueBadFiles);
    }
}