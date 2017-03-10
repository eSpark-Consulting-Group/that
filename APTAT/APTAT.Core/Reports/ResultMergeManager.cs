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
    public class FatResultMergeHelper : IResultMergeHelper
    {
        public FilesStatistics MergeResults(IEnumerable<IAptScanResult> flsResults, Dictionary<string, FileResult> valkyrieResults, out IEnumerable<ScannedTarget> mergedTargets, out IEnumerable<DetailedBadFile> uniqueBadFiles, out IEnumerable<DetailedBadFile> nonUniqueBadFiles)
        {
            throw new NotImplementedException();
        }
    }
    public class ResultMergeHelper : IResultMergeHelper
    {            
     
        

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

                int mergedSafeCount = 0;
                int mergedUnknownCount = 0;
                int mergedMalwareCount = 0;
                int mergedErrorFilesCount = 0;
                int mergedNoVerdictCount = 0;
                int mergedPendingFilesCount = 0;
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
                        if (badFile.Verdict != ValkyrieVerdict.Clean)
                        {
                            AddBadFile(uniqueFiles, badFile, module);
                            var copiedBadFile = new DetailedBadFile(module, new ScannedTarget(flsResult));
                            copiedBadFile.SetValkyrieAnalysis(fileResult.ValkyrieAnalysisResult);
                            nonUniqueBadFilesInternal.Add(copiedBadFile);
                        }
                        #region Calculate Statistics

                        do
                        {
                            if (module.Cloud == Descent.Apt.CloudVerdict.Malware)
                            {
                                mergedMalwareCount++;
                                break;
                            }
                            switch (fileResult.ValkyrieAnalysisResult.Verdict)
                            {
                                case ValkyrieVerdict.Clean:
                                    mergedSafeCount++;
                                    break;
                                case ValkyrieVerdict.Pending:
                                case ValkyrieVerdict.InQueue:
                                case ValkyrieVerdict.FirstSeen:
                                    mergedPendingFilesCount++;
                                    //      _pendingAnalysisFilesCount++;
                                    break;
                                case ValkyrieVerdict.Undetected:
                                    mergedUnknownCount++;
                                    break;
                                case ValkyrieVerdict.Malware:
                                case ValkyrieVerdict.PotentiallyUnwantedApplication:
                                    mergedMalwareCount++;
                                    break;
                                case ValkyrieVerdict.NotAnalyzed:
                                    {
                                        switch (module.Cloud)
                                        {
                                            case Descent.Apt.CloudVerdict.Safe:
                                                mergedSafeCount++;
                                                break;
                                            case Descent.Apt.CloudVerdict.Unknown:
                                            case Descent.Apt.CloudVerdict.FirstSeen:
                                                mergedUnknownCount++;
                                                break;
                                            case Descent.Apt.CloudVerdict.Malware:
                                                mergedMalwareCount++;
                                                break;
                                            case Descent.Apt.CloudVerdict.FailedToAnalyze:
                                            case Descent.Apt.CloudVerdict.NoVerdict:
                                                mergedErrorFilesCount++;
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
                        switch (module.Cloud)
                        {
                            case Descent.Apt.CloudVerdict.Safe:
                                mergedSafeCount++;
                                break;
                            case Descent.Apt.CloudVerdict.Unknown:
                            case Descent.Apt.CloudVerdict.FirstSeen:
                                mergedUnknownCount++;
                                break;
                            case Descent.Apt.CloudVerdict.Malware:
                                mergedMalwareCount++;
                                break;
                            case Descent.Apt.CloudVerdict.FailedToAnalyze:
                                mergedErrorFilesCount++;
                                break;
                            case Descent.Apt.CloudVerdict.NoVerdict:
                                mergedNoVerdictCount++;
                                break;

                            default:

                                throw new ArgumentOutOfRangeException();
                        }
                        #endregion
                    }
                }

                #endregion

                #region Filling in targets info


                ScannedTarget target = new ScannedTarget(flsResult)
                {
                    MalwareFilesCount = mergedMalwareCount,
                    SafeFilesCount = mergedSafeCount,
                    UnknownFilesCount = mergedUnknownCount,
                    FailedToAnalyzeFilesCount = mergedErrorFilesCount,
                    NoVerdictFilesCount = mergedNoVerdictCount,
                    PendingAnalysisFilesCount = mergedPendingFilesCount
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

                    badFile.Value.Targets[0].SafeFilesCount = mergedSafeCount;
                    badFile.Value.Targets[0].UnknownFilesCount = mergedUnknownCount;
                    badFile.Value.Targets[0].MalwareFilesCount = mergedMalwareCount;
                    badFile.Value.Targets[0].FailedToAnalyzeFilesCount = mergedErrorFilesCount;
                    badFile.Value.Targets[0].NoVerdictFilesCount = mergedNoVerdictCount;
                    badFile.Value.Targets[0].PendingAnalysisFilesCount = mergedPendingFilesCount;
                }

                safeFilesCount += mergedSafeCount;
                unknownFilesCount += mergedUnknownCount;
                malwareFilesCount += mergedMalwareCount;
                errorFilesCount += mergedErrorFilesCount;
                noVerdictFilesCount += mergedNoVerdictCount;
                totalFilesCount += flsResult.TotalFilesCount;
                emptyFilesCount += flsResult.EmptyFilesCount;
                skippedFilesCount += flsResult.SkippedFilesCount;
                pendingAnalysisFilesCount += mergedPendingFilesCount;
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

     
        private static string CalculateTargetVerdictString(ScannedTarget target)
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

    }
}