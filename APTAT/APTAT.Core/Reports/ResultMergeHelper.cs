using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.Core.Descent.Apt;

namespace APTAT.Core.Reports
{
    public class ResultMergeHelper
    {
        public virtual FilesStatistics MergeResults(IEnumerable<IAptScanResult> flsResults, out IEnumerable<ScannedTarget> mergedTargets,
          out IEnumerable<DetailedBadFile> uniqueBadFiles, out IEnumerable<DetailedBadFile> nonUniqueBadFiles)
        {
            int totalFilesCount = 0;
            int safeFilesCount = 0;
            int unknownFilesCount = 0;
            int malwareFilesCount = 0;
            int emptyFilesCount = 0;
            int skippedFilesCount = 0;
            int errorFilesCount = 0;
            int noVerdictFilesCount = 0;
            int pendingAnalysisFilesCount = 0;
            var targets = new List<ScannedTarget>();

            List<DetailedBadFile> nonUniqueBadFilesInternal = new List<DetailedBadFile>();
            Dictionary<string, DetailedBadFile> globalUniqueFiles = new Dictionary<string, DetailedBadFile>(StringComparer.OrdinalIgnoreCase);
            foreach (var flsResult in flsResults)
            {
                if (flsResult.ResultType != Core.ScanResultType.Completed)
                    continue;
                Dictionary<string, DetailedBadFile> uniqueFiles = new Dictionary<string, DetailedBadFile>(StringComparer.OrdinalIgnoreCase);

                var computerStatistics = new Statistics();
                #region Working with a single scan result

                foreach (var module in flsResult.Files)
                {

                    if (module.IsBad)
                    {
                        DetailedBadFile badFile = new DetailedBadFile(module, new ScannedTarget(flsResult));
                        AddBadFile(uniqueFiles, badFile, module);
                        DetailedBadFile copiedBadFile = new DetailedBadFile(module, new ScannedTarget(flsResult));
                        nonUniqueBadFilesInternal.Add(copiedBadFile);
                    }
                    CalculateStatistics(module, computerStatistics);

                }

                #endregion

                #region Filling in targets info
                ScannedTarget target = new ScannedTarget(flsResult)
                {
                    MalwareFilesCount = computerStatistics.MergedMalwareCount,
                    SafeFilesCount = computerStatistics.MergedSafeCount,
                    UnknownFilesCount = computerStatistics.MergedUnknownCount,
                    FailedToAnalyzeFilesCount = computerStatistics.MergedErrorFilesCount,
                    NoVerdictFilesCount = computerStatistics.MergedNoVerdictCount,
                    PendingAnalysisFilesCount = computerStatistics.MergedPendingFilesCount
                };
                target.TargetVerdictString = CalculateTargetVerdictString(target);

                targets.Add(target);

                #endregion

                foreach (var badFile in uniqueFiles)
                {
                    if (globalUniqueFiles.ContainsKey(badFile.Key))
                    {
                        globalUniqueFiles[badFile.Key].Targets.AddRange(badFile.Value.Targets);
                    }
                    else
                    {
                        globalUniqueFiles.Add(badFile.Key, badFile.Value);
                    }

                    badFile.Value.Targets[0].SafeFilesCount = computerStatistics.MergedSafeCount;
                    badFile.Value.Targets[0].UnknownFilesCount = computerStatistics.MergedUnknownCount;
                    badFile.Value.Targets[0].MalwareFilesCount = computerStatistics.MergedMalwareCount;
                    badFile.Value.Targets[0].FailedToAnalyzeFilesCount = computerStatistics.MergedErrorFilesCount;
                    badFile.Value.Targets[0].NoVerdictFilesCount = computerStatistics.MergedNoVerdictCount;
                    badFile.Value.Targets[0].PendingAnalysisFilesCount = computerStatistics.MergedPendingFilesCount;
                }

                safeFilesCount += computerStatistics.MergedSafeCount;
                unknownFilesCount += computerStatistics.MergedUnknownCount;
                malwareFilesCount += computerStatistics.MergedMalwareCount;
                errorFilesCount += computerStatistics.MergedErrorFilesCount;
                noVerdictFilesCount += computerStatistics.MergedNoVerdictCount;
                totalFilesCount += flsResult.TotalFilesCount;
                emptyFilesCount += flsResult.EmptyFilesCount;
                skippedFilesCount += flsResult.SkippedFilesCount;
                pendingAnalysisFilesCount += computerStatistics.MergedPendingFilesCount;
            }

            mergedTargets = targets;

            var statistics = new FilesStatistics
            {
                EmptyFilesCount = emptyFilesCount,
                MalwareFilesCount = malwareFilesCount,
                SafeFilesCount = safeFilesCount,
                TotalFilesCount = totalFilesCount,
                UnknownFilesCount = unknownFilesCount,
                SkippedFilesCount = skippedFilesCount,
                FailedToAnalyzeFilesCount = errorFilesCount,
                NoVerdictFilesCount = noVerdictFilesCount,
                PendingAnalysisFilesCount = pendingAnalysisFilesCount
            };

            nonUniqueBadFiles = nonUniqueBadFilesInternal;
            uniqueBadFiles = globalUniqueFiles.Values.ToArray();
            return statistics;
        }

        protected virtual void CalculateStatistics(IModule module, Statistics computerStatistics)
        {
            switch (module.Cloud)
            {
                case CloudVerdict.Safe:
                    computerStatistics.MergedSafeCount++;
                    break;
                case CloudVerdict.Unknown:
                case CloudVerdict.FirstSeen:
                    computerStatistics.MergedUnknownCount++;
                    break;
                case CloudVerdict.Malware:
                    computerStatistics.MergedMalwareCount++;
                    break;
                case CloudVerdict.FailedToAnalyze:
                    computerStatistics.MergedErrorFilesCount++;
                    break;
                case CloudVerdict.NoVerdict:
                    computerStatistics.MergedNoVerdictCount++;
                    break;

                default:

                    throw new ArgumentOutOfRangeException();
            }
        }
        protected virtual string CalculateTargetVerdictString(ScannedTarget target)
        {
            if (target.MalwareFilesCount > 0)
            {
                return "Infected";
            }

            if (target.UnknownFilesCount > 0 || target.FailedToAnalyzeFilesCount > 0 || target.NoVerdictFilesCount > 0 || target.PendingAnalysisFilesCount > 0)
            {
                return "At Risk";
            }

            if (target.TotalFilesCount == 0)
            {
                return "Unknown";
            }

            return "Safe";
        }
        private void AddBadFile(Dictionary<string, DetailedBadFile> uniqueFiles, DetailedBadFile badFile, IModule module)
        {
            if (uniqueFiles.ContainsKey(badFile.Sha1))
            {
                uniqueFiles[badFile.Sha1].Targets[0].BadFileInstances.Add(module.Path);
            }
            else
            {
                uniqueFiles.Add(badFile.Sha1, badFile);
            }
        }
        protected class Statistics
        {
            public int MergedSafeCount { get; set; }
            public int MergedUnknownCount { get; set; }
            public int MergedMalwareCount { get; set; }
            public int MergedErrorFilesCount { get; set; }
            public int MergedNoVerdictCount { get; set; }
            public int MergedPendingFilesCount { get; set; }
        }

    }
}