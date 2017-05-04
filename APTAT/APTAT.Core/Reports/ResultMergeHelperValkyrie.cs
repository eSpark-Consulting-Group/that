using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Valkyrie;

namespace APTAT.Core.Reports
{
    public class ResultMergeHelperValkyrie : ResultMergeHelper
    {
        protected IFinalVerdictCalculator Calculator;
        public ResultMergeHelperValkyrie(IFinalVerdictCalculator calculator)
        {
            Calculator = calculator;
        }

        public FilesStatistics MergeResults(IEnumerable<IAptScanResult> flsResults, Dictionary<string, IValkyrieAnalysisResult> valkyrieResults, out IEnumerable<ScannedTarget> mergedTargets,
            out IEnumerable<DetailedBadFileWithValkyrie> uniqueBadFiles, out IEnumerable<DetailedBadFileWithValkyrie> nonUniqueBadFiles)
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

            List<DetailedBadFileWithValkyrie> nonUniqueBadFilesInternal = new List<DetailedBadFileWithValkyrie>();
            Dictionary<string, DetailedBadFileWithValkyrie> globalUniqueFiles = new Dictionary<string, DetailedBadFileWithValkyrie>(StringComparer.OrdinalIgnoreCase);
            foreach (var flsResult in flsResults)
            {
                if (flsResult.ResultType != Core.ScanResultType.Completed)
                    continue;
                Dictionary<string, DetailedBadFileWithValkyrie> uniqueFiles = new Dictionary<string, DetailedBadFileWithValkyrie>(StringComparer.OrdinalIgnoreCase);

                var computerStatistics = new Statistics();
                #region Working with a single scan result

                foreach (var module in flsResult.Files)
                {
                    IValkyrieAnalysisResult valkyrieAnalysisResult = null;
                    if (module.IsBad)
                    {
                        valkyrieResults.TryGetValue(module.Sha1, out valkyrieAnalysisResult);
                    }
                    if (valkyrieAnalysisResult != null)
                    {
                        var badFile = new DetailedBadFileWithValkyrie(module, new ScannedTarget(flsResult), Calculator, valkyrieAnalysisResult);


                        if (badFile.Verdict != ValkyrieVerdict.Clean)
                        {
                            AddBadFile(uniqueFiles, badFile, module);
                            var copiedBadFile = new DetailedBadFileWithValkyrie(module, new ScannedTarget(flsResult), Calculator, valkyrieAnalysisResult);
                            nonUniqueBadFilesInternal.Add(copiedBadFile);
                        }

                        CalculateStatistics(module, computerStatistics, valkyrieAnalysisResult.Verdict);

                    }
                    else
                    {
                        if (module.IsBad)
                        {
                            DetailedBadFileWithValkyrie badFile2 = new DetailedBadFileWithValkyrie(module, new ScannedTarget(flsResult), Calculator,new ValkyrieAnalysisResult(module.Sha1, ValkyrieVerdict.InQueue));
                            AddBadFile(uniqueFiles, badFile2, module);
                            DetailedBadFileWithValkyrie copiedBadFile = new DetailedBadFileWithValkyrie(module, new ScannedTarget(flsResult), Calculator, new ValkyrieAnalysisResult(module.Sha1, ValkyrieVerdict.InQueue));
                            nonUniqueBadFilesInternal.Add(copiedBadFile);
                        }
                        #region Calculate Statistics
                        CalculateStatistics(module, computerStatistics);
                        #endregion
                    }
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

            var statistics = new FilesStatistics()
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

        private void AddBadFile(Dictionary<string, DetailedBadFileWithValkyrie> uniqueFiles, DetailedBadFileWithValkyrie badFile, IModule module)
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

        protected virtual void CalculateStatistics(IModule module, Statistics computerStatistics, ValkyrieVerdict verdict)
        {
            do
            {
                if (module.Cloud == Descent.Apt.CloudVerdict.Malware)
                {
                    computerStatistics.MergedMalwareCount++;
                    break;
                }
                switch (verdict)
                {
                    case ValkyrieVerdict.Clean:
                        computerStatistics.MergedSafeCount++;
                        break;
                    case ValkyrieVerdict.Pending:
                    case ValkyrieVerdict.FirstSeen:
                        computerStatistics.MergedPendingFilesCount++;
                        //      _pendingAnalysisFilesCount++;
                        break;
                    case ValkyrieVerdict.Undetected:
                        computerStatistics.MergedUnknownCount++;
                        break;
                    case ValkyrieVerdict.Malware:
                    case ValkyrieVerdict.PotentiallyUnwantedApplication:
                        computerStatistics.MergedMalwareCount++;
                        break;
                    case ValkyrieVerdict.InQueue:
                    case ValkyrieVerdict.NotAnalyzed:
                    {
                        switch (module.Cloud)
                        {
                            case Descent.Apt.CloudVerdict.Safe:
                                computerStatistics.MergedSafeCount++;
                                break;
                            case Descent.Apt.CloudVerdict.Unknown:
                            case Descent.Apt.CloudVerdict.FirstSeen:
                                computerStatistics.MergedUnknownCount++;
                                break;
                            case Descent.Apt.CloudVerdict.Malware:
                                computerStatistics.MergedMalwareCount++;
                                break;
                            case Descent.Apt.CloudVerdict.FailedToAnalyze:
                            case Descent.Apt.CloudVerdict.NoVerdict:
                                computerStatistics.MergedErrorFilesCount++;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            } while (false);
        }
    }
}