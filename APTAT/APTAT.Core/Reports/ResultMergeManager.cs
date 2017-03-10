using APTAT.Core.Descent.Apt;
using APTAT.Valkyrie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APTAT.Core.Reports.AptReportData;

namespace APTAT.Core.Reports
{
    public class FatResultMergeHelper : ResultMergeHelper
    {
        public FatResultMergeHelper(IFinalVerdictCalculator calc)
            : base(calc)
        {

        }
        protected override void CalculateStatistics(IModule module, Statistics computerStatistics)
        {
            var final = _calculator.GetFinalVerdict(module.Cloud, ValkyrieVerdict.InQueue);
            UpdateStatisticsByFinalVerdict(computerStatistics, final);
        }

        private static void UpdateStatisticsByFinalVerdict(Statistics computerStatistics, FinalVerdict final)
        {
            switch (final)
            {
                case FinalVerdict.Clean:
                    computerStatistics.MergedSafeCount++;
                    break;
                case FinalVerdict.Malware:
                    computerStatistics.MergedMalwareCount++;
                    break;
                case FinalVerdict.Unknown:
                    computerStatistics.MergedUnknownCount++;
                    break;
                case FinalVerdict.NotAnalyzed:
                    computerStatistics.MergedErrorFilesCount++;
                    break;
                case FinalVerdict.InProgress:
                    computerStatistics.MergedPendingFilesCount++;
                    break;
                default:
                    break;
            }
        }

        protected override void CalculateStatistics(IModule module, FileResult fileResult, Statistics computerStatistics)
        {
            var final = _calculator.GetFinalVerdict(module.Cloud, fileResult.ValkyrieAnalysisResult.Verdict);
            UpdateStatisticsByFinalVerdict(computerStatistics, final);
        }
    }
    public class ResultMergeHelper : IResultMergeHelper
    {
        protected readonly IFinalVerdictCalculator _calculator;

        public ResultMergeHelper(IFinalVerdictCalculator calculator)
        {
            _calculator = calculator;
        }

        public FilesStatistics MergeResults(IEnumerable<IAptScanResult> flsResults, Dictionary<string, FileResult> valkyrieResults, out IEnumerable<ScannedTarget> mergedTargets,
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
                    FileResult fileResult = null;
                    if (module.IsBad)
                    {
                        valkyrieResults.TryGetValue(module.Sha1, out fileResult);
                    }
                    if (fileResult != null)
                    {
                        DetailedBadFile badFile = new DetailedBadFile(module, new ScannedTarget(flsResult));

                        badFile.SetValkyrieAnalysis(fileResult.ValkyrieAnalysisResult);
                        badFile.FinalVerdict = _calculator.GetFinalVerdict(badFile.Cloud, fileResult.ValkyrieAnalysisResult.Verdict);
                        badFile.FinalVerdictString = _calculator.GetVerdictString(badFile.FinalVerdict);

                        if (badFile.Verdict != ValkyrieVerdict.Clean)
                        {
                            AddBadFile(uniqueFiles, badFile, module);
                            var copiedBadFile = new DetailedBadFile(module, new ScannedTarget(flsResult));
                            copiedBadFile.SetValkyrieAnalysis(fileResult.ValkyrieAnalysisResult);
                            nonUniqueBadFilesInternal.Add(copiedBadFile);
                        }
                        #region Calculate Statistics

                        CalculateStatistics(module, fileResult, computerStatistics);


                        #endregion
                    }
                    else
                    {
                        if (module.IsBad)
                        {
                            DetailedBadFile badFile = new DetailedBadFile(module, new ScannedTarget(flsResult));
                            AddBadFile(uniqueFiles, badFile, module);
                            DetailedBadFile copiedBadFile = new DetailedBadFile(module, new ScannedTarget(flsResult));
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

        protected virtual void CalculateStatistics(IModule module, Statistics computerStatistics)
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
                    computerStatistics.MergedErrorFilesCount++;
                    break;
                case Descent.Apt.CloudVerdict.NoVerdict:
                    computerStatistics.MergedNoVerdictCount++;
                    break;

                default:

                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual void CalculateStatistics(IModule module, FileResult fileResult, Statistics computerStatistics)
        {
            do
            {
                if (module.Cloud == Descent.Apt.CloudVerdict.Malware)
                {
                    computerStatistics.MergedMalwareCount++;
                    break;
                }
                switch (fileResult.ValkyrieAnalysisResult.Verdict)
                {
                    case ValkyrieVerdict.Clean:
                        computerStatistics.MergedSafeCount++;
                        break;
                    case ValkyrieVerdict.Pending:
                    case ValkyrieVerdict.InQueue:
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
                uniqueFiles[badFile.Sha1].Targets[0].BadFileInstances.Add(new DetailedBadFile.BadFileInstanceInfo(module));
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