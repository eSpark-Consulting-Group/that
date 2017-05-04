using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Valkyrie;

namespace APTAT.Core.Reports
{
    public class AptReportDataValkyrie: AptReportData
    {
        protected IFinalVerdictCalculator Calculator { get; }

        private ResultMergeHelperValkyrie _mergeHelper;

        protected virtual ResultMergeHelperValkyrie MergeHelper => _mergeHelper ?? (_mergeHelper = new ResultMergeHelperValkyrie(Calculator));

        public AptReportDataValkyrie(ReportData reportData, IFinalVerdictCalculator calculator) : base(reportData)
        {
            Calculator = calculator;

            ValkyrieAnalysisResult = reportData.ValkyrieAnalysisResult;
        }
        protected override DetailedBadFile[] GetAllBadFilesSorted(bool isUniqueSetOfFiles = true)
        {
            IEnumerable<ScannedTarget> targets;
            IEnumerable<DetailedBadFileWithValkyrie> allUniqueBadFiles;
            IEnumerable<DetailedBadFileWithValkyrie> allNonUniqueBadFiles;
            if (_Targets == null) throw new InvalidOperationException("Targets required");
            MergeResults(_Targets, ValkyrieAnalysisResult, out targets, out allUniqueBadFiles, out allNonUniqueBadFiles);
            
            return (isUniqueSetOfFiles ? allUniqueBadFiles : allNonUniqueBadFiles)
                .OrderByDescending(file => file.InstancesTotalCount).ThenBy(file => file.Name).ToArray();
        }
        public Dictionary<string, IValkyrieAnalysisResult> ValkyrieAnalysisResult { get; set; }

        private DetailedBadFileWithValkyrie[] _allBadFilesSorted;
        public new DetailedBadFileWithValkyrie[] AllBadFilesSorted
        {
            get
            {
                if (_allBadFilesSorted == null)
                {
                    _allBadFilesSorted = GetAllBadFilesSorted().Cast<DetailedBadFileWithValkyrie>().ToArray();
                }

                return _allBadFilesSorted;
            }

        }
        public override ItemRatingInfo[] DevicesRating
        {
            get
            {
                IEnumerable<ScannedTarget> targets;
                IEnumerable<DetailedBadFileWithValkyrie> files;
                MergeResults(_Targets, ValkyrieAnalysisResult, out targets, out files, out files);
                return CalculateTargetVerdict(targets);
            }
        }
        private MergedScanResults<DetailedBadFileWithValkyrie> _mergedScanResultsCache;
        

        protected virtual void MergeResults(IEnumerable<IAptScanResult> flsResults, Dictionary<string, IValkyrieAnalysisResult> valkyrieResults, out IEnumerable<ScannedTarget> mergedTargets,
            out IEnumerable<DetailedBadFileWithValkyrie> uniqueBadFiles, out IEnumerable<DetailedBadFileWithValkyrie> nonUniqueBadFiles)
        {
        
            if (_mergedScanResultsCache != null)
            {
                mergedTargets = _mergedScanResultsCache.MergedTargets;
                uniqueBadFiles = _mergedScanResultsCache.MergeUniqueResults;
                nonUniqueBadFiles = _mergedScanResultsCache.MergeNonUniqueResults;
                return;
            }
            var statistics = MergeHelper.MergeResults(flsResults, valkyrieResults, out mergedTargets, out uniqueBadFiles, out nonUniqueBadFiles);
            if (statistics != null) ReplaceStatistics(statistics);
            _mergedScanResultsCache = new MergedScanResults<DetailedBadFileWithValkyrie>
            {
                MergeUniqueResults = uniqueBadFiles,
                MergeNonUniqueResults = nonUniqueBadFiles,
                MergedTargets = mergedTargets
            };
        }

        public override void CalculateTotal()
        {
            IEnumerable<ScannedTarget> targets = new ScannedTarget[] { };
            IEnumerable<DetailedBadFileWithValkyrie> files;
            MergeResults(_Targets, ValkyrieAnalysisResult, out targets, out files, out files);

        }

        public override VulnerableDeviceInfoEx[] Top10VulnerableDevices
        {
            get
            {
                IEnumerable<ScannedTarget> targets = new ScannedTarget[] { };
                IEnumerable<DetailedBadFileWithValkyrie> files;
                MergeResults(_Targets, ValkyrieAnalysisResult, out targets, out files, out files);

                var top10 = (from target in targets
                             where target.BadFilesCount > 0
                             orderby target.BadFilesCount descending
                             select target).Take(10);

                List<VulnerableDeviceInfoEx> devices = new List<VulnerableDeviceInfoEx>();
                foreach (var scanResult in top10)
                {
                    if (scanResult.UnknownFilesCount > 0)
                        devices.Add(new VulnerableDeviceInfoEx { BadFilesCount = scanResult.UnknownFilesCount, BadFileType = "Unknown", Name = scanResult.HostName, Uid = scanResult.TargetUid });
                    if (scanResult.MalwareFilesCount > 0)
                        devices.Add(new VulnerableDeviceInfoEx { BadFilesCount = scanResult.MalwareFilesCount, BadFileType = "Infected", Name = scanResult.HostName, Uid = scanResult.TargetUid });
                    if (scanResult.NotAnalyzedFilesCount > 0)
                        devices.Add(new VulnerableDeviceInfoEx { BadFilesCount = scanResult.NotAnalyzedFilesCount, BadFileType = "Not Analyzed", Name = scanResult.HostName, Uid = scanResult.TargetUid });
                    if (scanResult.PendingAnalysisFilesCount > 0)
                        devices.Add(new VulnerableDeviceInfoEx { BadFilesCount = scanResult.PendingAnalysisFilesCount, BadFileType = "Pending", Name = scanResult.HostName, Uid = scanResult.TargetUid });
                }

                return devices.ToArray();
            }
        }
        public override ItemRatingInfo[] FileRating
        {
            get
            {
                var total = SafeFilesCount + UnknownFilesFound + MalwareFilesFound + NotAnalyzedFilesCount + PendingAnalysisFilesCount;

                var itemRatingInfoList = new List<ItemRatingInfo>
                {
                    new FileRatingValkyrieInfo { CloudVerdict = APTAT.Core.Valkyrie.ValkyrieVerdict.Clean, Count = SafeFilesCount, Value =
                        $"Trusted ({SafeFilesCount / (double) total:P2})"
                    },
                    new FileRatingValkyrieInfo { CloudVerdict = APTAT.Core.Valkyrie.ValkyrieVerdict.Undetected, Count = UnknownFilesFound, Value =
                        $"Unknown  ({UnknownFilesFound / (double) total:P2})"
                    },
                    new FileRatingValkyrieInfo { CloudVerdict = APTAT.Core.Valkyrie.ValkyrieVerdict.Malware, Count = MalwareFilesFound, Value =
                        $"Malicious ({MalwareFilesFound / (double) total:P2})"
                    },
                    new FileRatingValkyrieInfo { CloudVerdict = APTAT.Core.Valkyrie.ValkyrieVerdict.NotAnalyzed, Count = NotAnalyzedFilesCount, Value =
                        $"Lookup In Progress ({NotAnalyzedFilesCount / (double) total:P2})"
                    },
                    new FileRatingValkyrieInfo { CloudVerdict = APTAT.Core.Valkyrie.ValkyrieVerdict.Pending, Count = PendingAnalysisFilesCount, Value =
                        $"In Analysis ({PendingAnalysisFilesCount / (double) total:P2})"
                    }
                };

                return itemRatingInfoList.ToArray();
            }
        }
        public override ScanResult[] SucceededTargets
        {
            get
            {
                IEnumerable<ScannedTarget> targets;
                IEnumerable<DetailedBadFileWithValkyrie> files;
                MergeResults(_Targets, ValkyrieAnalysisResult, out targets, out files, out files);
                DetailedBadFileWithValkyrie[] detailedBadFiles =  files.ToArray();

                List<ScanResultValkyrie> scanResults = new List<ScanResultValkyrie>();
                foreach (var t in targets)
                {
                    DetailedBadFileWithValkyrie[] endPointFiles = (from file in detailedBadFiles where file.Targets.Any(tg => tg.TargetUid == t.TargetUid) select file).ToArray();
                    ScanResultType resultType = t.ResultType.ToBusiness();
                    var scanResult = new ScanResultValkyrie
                    {
                        HostName = t.Computer.Name,
                        IPAddress = t.Computer.IPAddress,
                        OsName = t.AptReport?.Computer.Os.ToString(),
                        OrganizationUnit = t.Computer.OrganizationUnit,
                        RunningProgramsCount = t.AptReport?.Processes?.Length ?? 0,
                        AutorunsCount = t.AptReport?.Autoruns?.Length ?? 0,
                        MalwareFilesCount = t.MalwareFilesCount,
                        UnknownFilesCount = t.UnknownFilesCount,
                        SafeFilesCount = t.SafeFilesCount,
                        ResultType = resultType,
                        ResultErrorText = t.ResultErrorText,
                        TotalFilesCount = t.TotalFilesCount,
                        FailedToAnalyzeFilesCount = t.FailedToAnalyzeFilesCount,
                        NoVerdictFilesCount = t.NoVerdictFilesCount,
                        PendingAnalysisFilesCount = t.PendingAnalysisFilesCount,
                        UknownFilesValkyrie = endPointFiles.Where(f => f.FileType == BadFileType.Unknown).ToArray(),
                        MaliciousFilesValkyrie = endPointFiles.Where(f => f.FileType == BadFileType.Infected).ToArray(),
                        FailedToAnalyzeFilesValkyrie = endPointFiles.Where(f => f.FileType == BadFileType.NotAnalyzed).ToArray(),
                        PendingAnalysisFilesValkyrie = endPointFiles.Where(f => f.FileType == BadFileType.Pending).ToArray(),
                        EndPointFiles = endPointFiles.ToArray()
                    };

                    scanResults.Add(scanResult);
                }
                var result = scanResults.OrderBy(t => t.SeverityIndex).ToArray();
                return result;
            }
        }

    }
}
