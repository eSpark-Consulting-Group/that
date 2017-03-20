using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Directory;
using APTAT.Valkyrie;
using Valkyrie.WebApiProvider.Enums;
using ValkyrieVerdict = APTAT.Valkyrie.ValkyrieVerdict;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;
namespace APTAT.Core.Reports
{
    public class AptReportData
    {
        public AptReportData(IResultMergeHelper mergeHelper)
        {
            _Targets = new IAptScanResult[] { };
            _mergeHelper = mergeHelper;
        }
        private MergedScanResults _mergedScanResultsCache;

        readonly IResultMergeHelper _mergeHelper;

        private int _safeFilesCount = -1;
        private int _unknownFilesCount = -1;
        private int _malwareFilesCount = -1;

        public Guid OperationUid { get; set; }
        public Guid ValkyrieOperationUid { get; set; }
        private int _emptyFilesCount = -1;
        private int _skippedFilesCount = -1;
        private int _errorFileCount = -1;
        private int _noVerdictFileCount = -1;
        private int _totalFilesCound = -1;
        private int _pendingAnalysisFilesCount = -1;

        public DateTime ScanStarted { get; set; }
        public DateTime ScanFinished { get; set; }
        public int TotalTargetsCount { get; set; }
        public IDevicesSourceDescription DevicesSourceDescription { get; set; }
        public string DevicesSourceDescriptionString
        {
            get
            {
                if (DevicesSourceDescription == null) return String.Empty;
                switch (DevicesSourceDescription.Type)
                {
                    case ETargetsDiscovererType.ActiveDirectory:
                        return $"Active Directory ({DevicesSourceDescription.Description})";
                    case ETargetsDiscovererType.NetworkAddresses:
                        return $"Network Addresses ({DevicesSourceDescription.Description}) ";
                    case ETargetsDiscovererType.Workgroup:
                        return $"Workgroup ({DevicesSourceDescription.Description})";
                    case ETargetsDiscovererType.LocalComputer:
                        return $"Local Computer ({DevicesSourceDescription.Description})";
                    default:
                        return string.Empty;
                }
            }
        }
        public bool IsActiveDirectoryDevicesSource { get { return DevicesSourceDescription.Type == ETargetsDiscovererType.ActiveDirectory; } }


        public int TargetsNotPerformed { get; set; }
        public int TargetsScanned
        {
            get { return TotalTargetsCount - TargetsNotPerformed; }
        }
        public int TargetsSucceeded { get; set; }
        public int TargetsFailed { get; set; }

        public int TotalFilesScanned
        {
            get
            {
                if (_totalFilesCound == -1)
                    CalculateTotal();
                return _totalFilesCound;
            }
        }

        public int SafeFilesCount
        {
            get
            {
                if (_safeFilesCount == -1)
                    CalculateTotal();
                return _safeFilesCount;
            }
        }

        public int UnknownFilesFound
        {
            get
            {
                if (_unknownFilesCount == -1)
                    CalculateTotal();
                return _unknownFilesCount;
            }
        }

        public int MalwareFilesFound
        {
            get
            {
                if (_malwareFilesCount == -1)
                    CalculateTotal();
                return _malwareFilesCount;
            }
        }

        public int EmptyFilesFound
        {
            get
            {
                if (_emptyFilesCount == -1)
                    CalculateTotal();
                return _emptyFilesCount;
            }
        }

        public int FailedToAnalyzeFilesCount
        {
            get
            {
                if (_errorFileCount == -1)
                    CalculateTotal();
                return _errorFileCount;
            }
        }

        public int LookupInProgressFilesCount => FailedToAnalyzeFilesCount + NoVerdictFilesCount;

        public int NotAnalyzedFilesCount
        {
            get
            {
                if (_errorFileCount == -1 || _noVerdictFileCount == -1)
                    CalculateTotal();
                return _errorFileCount + _noVerdictFileCount;
            }
        }


        public int NoVerdictFilesCount
        {
            get
            {
                if (_noVerdictFileCount == -1)
                    CalculateTotal();
                return _noVerdictFileCount;
            }
        }

        public int InAnalysisFilesCount
        {
            get
            {
                return PendingAnalysisFilesCount + LookupInProgressFilesCount;
            }
        }

        public int PendingAnalysisFilesCount
        {
            get
            {
                if (_pendingAnalysisFilesCount == -1)
                    CalculateTotal();
                return _pendingAnalysisFilesCount;
            }
        }
        public int TotalErrorFilesCount
        {
            get
            {
                if (_noVerdictFileCount == -1 || _errorFileCount == -1)
                {
                    CalculateTotal();
                }
                return _noVerdictFileCount + _errorFileCount;
            }

        }
        public int SkippedFilesCount
        {
            get
            {
                if (_skippedFilesCount == -1)
                    CalculateTotal();
                return _skippedFilesCount;
            }
        }

        public AptRiskAssessmentLevel TargetsSafetyLevel
        {
            get
            {
                if (TargetsSucceeded == 0)
                    return AptRiskAssessmentLevel.Unknown;
                if (MalwareFilesFound > 0)
                    return AptRiskAssessmentLevel.High;
                if (UnknownFilesFound > 0 || FailedToAnalyzeFilesCount > 0 || NoVerdictFilesCount > 0 || PendingAnalysisFilesCount > 0)
                    return AptRiskAssessmentLevel.Medium;
                return AptRiskAssessmentLevel.Ok;
            }
        }

        public string TargetsSafetyLevelString
        {
            get { return TargetsSafetyLevel.ToString(); }
        }

        public int NetworkTrustLevel
        {
            get
            {
                if (TotalFilesScanned == 0)
                    return -1;

                int total = TotalFilesScanned /*- EmptyFilesFound*/;
                if (total <= 0)
                    return -1;

                return (int) ((double) SafeFilesCount / total * 100);
            }
        }

        public string NetworkTrustLevelString
        {
            get { return NetworkTrustLevel >= 0 ? String.Format("{0} %", NetworkTrustLevel) : "Unknown"; }
        }

        public void SetTargets(IAptScanResult[] targets)
        {
            _Targets = targets;
        }
        private IAptScanResult[] _Targets;

        public ScanResult[] SucceededTargets
        {
            get
            {
                var targets = (from t in _Targets where t.ResultType == Core.ScanResultType.Completed select t).ToArray();
                List<ScanResult> scanResults = new List<ScanResult>();
                foreach (var t in targets)
                {
                    ScanResultType resultType = MapScanResult(t.ResultType);
                    var scanResult = new ScanResult()
                    {
                        HostName = t.Computer.Name,
                        IPAddress = t.Computer.IPAddress,
                        OsName = t.AptReport?.Computer.Os.ToString(),
                        OrganizationUnit = t.Computer.OrganizationUnit,
                        RunningProgramsCount = t.AptReport.Processes != null ? t.AptReport.Processes.Length : 0,
                        AutorunsCount = t.AptReport.Autoruns != null ? t.AptReport.Autoruns.Length : 0,
                        MalwareFilesCount = t.MalwareFilesCount,
                        UnknownFilesCount = t.UnknownFilesCount,
                        SafeFilesCount = t.SafeFilesCount,
                        ResultType = resultType,
                        ResultErrorText = t.ResultErrorText,
                        TotalFilesCount = t.TotalFilesCount,
                        FailedToAnalyzeFilesCount = t.FailedToAnalyzeFilesCount,
                        NoVerdictFilesCount = t.NoVerdictFilesCount,
                        PendingAnalysisFilesCount = t.PendingAnalysisFilesCount,
                        MalwareFiles = t.MalwareFiles.Select(s=>s.Map()).DefaultIfEmpty().ToArray(),
                        UnknownFiles = t.UnknownFiles.Select(s => s.Map()).DefaultIfEmpty().ToArray(),
                        LookupInProgressFiles = t.LookupInProgressFiles.Select(s => s.Map()).DefaultIfEmpty().ToArray()
                    };

                    scanResults.Add(scanResult);
                }
                return scanResults.ToArray();
            }
        }

        public ScanResultValkyrie[] SucceededTargetsWithValkyrie
        {
            get
            {
                IEnumerable<ScannedTarget> targets;
                IEnumerable<DetailedBadFile> files;
                MergeResults(_Targets, ValkyrieAnalysisResult, out targets, out files, out files);

                List<ScanResultValkyrie> scanResults = new List<ScanResultValkyrie>();
                foreach (var t in targets)
                {
                    var endPointFiles = (from file in files where file.Targets.Any(tg => tg.TargetUid == t.TargetUid) select file).ToArray();
                    ScanResultType resultType = MapScanResult(t.ResultType);
                    var scanResult = new ScanResultValkyrie()
                    {
                        HostName = t.Computer.Name,
                        IPAddress = t.Computer.IPAddress,
                        OsName = t.AptReport?.Computer.Os.ToString(),
                        OrganizationUnit = t.Computer.OrganizationUnit,
                        RunningProgramsCount = t.AptReport.Processes != null ? t.AptReport.Processes.Length : 0,
                        AutorunsCount = t.AptReport.Autoruns != null ? t.AptReport.Autoruns.Length : 0,
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


                    };

                    scanResults.Add(scanResult);
                }

                return scanResults.ToArray();
            }
        }

        private static ScanResultType MapScanResult(Core.ScanResultType scanResultType)
        {
            ScanResultType resultType = ScanResultType.NotPerformed;
            switch (scanResultType)
            {
                case Core.ScanResultType.NotPerformed:
                    resultType = ScanResultType.NotPerformed;
                    break;
                case Core.ScanResultType.InProgress:
                    throw new NotSupportedException(nameof(Core.ScanResultType.InProgress));
                case Core.ScanResultType.Completed:
                    resultType = ScanResultType.Completed;
                    break;
                case Core.ScanResultType.Failed:
                    resultType = ScanResultType.Failed;
                    break;
                case Core.ScanResultType.Canceled:
                    resultType = ScanResultType.Canceled;
                    break;
                default:
                    break;
            }

            return resultType;
        }

        private void CalculateTotal()
        {
            if (_Targets == null) return;
            var statistics = StatisticsCalculator.CalculateFilesStatistics(_Targets);

            ReplaceStatistics(statistics);
        }

        private void ReplaceStatistics(IFilesStatistics statistics)
        {
            _safeFilesCount = statistics.SafeFilesCount;
            _unknownFilesCount = statistics.UnknownFilesCount;
            _malwareFilesCount = statistics.MalwareFilesCount;
            _emptyFilesCount = statistics.EmptyFilesCount;
            _skippedFilesCount = statistics.SkippedFilesCount;
            _errorFileCount = statistics.FailedToAnalyzeFilesCount;
            _noVerdictFileCount = statistics.NoVerdictFilesCount;
            _totalFilesCound = statistics.TotalFilesCount;
            _pendingAnalysisFilesCount = statistics.PendingAnalysisFilesCount;
        }



        public ItemRatingInfo[] FileRating
        {
            get
            {
                var total = SafeFilesCount + UnknownFilesFound + MalwareFilesFound + LookupInProgressFilesCount;

                var itemRatingInfoList = new List<ItemRatingInfo>()
                {
                    new FileRatingInfo(){CloudVerdict = CloudVerdict.Safe, Count = SafeFilesCount, Value = string.Format("Trusted: ({0:P2})", SafeFilesCount  / (double)total)},
                    new FileRatingInfo(){CloudVerdict = CloudVerdict.Unknown, Count = UnknownFilesFound, Value = string.Format("Unknown  ({0:P2})", UnknownFilesFound / (double) total)},
                    new FileRatingInfo() { CloudVerdict = CloudVerdict.Malware, Count = MalwareFilesFound, Value = string.Format("Malicious ({0:P2})", MalwareFilesFound / (double) total)},
                    new FileRatingInfo() { CloudVerdict = CloudVerdict.FailedToAnalyze, Count = LookupInProgressFilesCount, Value = string.Format("Lookup In Progress ({0:P2})" , LookupInProgressFilesCount / (double) total)},
                    //new FileRatingInfo() { CloudVerdict = CloudVerdict.NoVerdict, Count = NoVerdictFilesCount, Value = "No Verdict " }
                };

                return itemRatingInfoList.ToArray();
            }
        }

        public ItemRatingInfo[] FileRatingFAT
        {
            get
            {
                var total = SafeFilesCount + UnknownFilesFound + MalwareFilesFound + InAnalysisFilesCount;

                var itemRatingInfoList = new List<ItemRatingInfo>
                {
                    new FileRatingValkyrieInfo { CloudVerdict = ValkyrieVerdict.Clean, Count = SafeFilesCount, Value = string.Format("Trusted: ({0:P2})", SafeFilesCount / (double) total)},
                    new FileRatingValkyrieInfo { CloudVerdict = ValkyrieVerdict.Undetected, Count = UnknownFilesFound, Value = string.Format("Unknown  ({0:P2})", UnknownFilesFound / (double) total)},
                    new FileRatingValkyrieInfo { CloudVerdict = ValkyrieVerdict.Malware, Count = MalwareFilesFound, Value = string.Format("Malicious ({0:P2})", MalwareFilesFound / (double) total)},                    
                    new FileRatingValkyrieInfo { CloudVerdict = ValkyrieVerdict.Pending, Count = InAnalysisFilesCount, Value = string.Format("In Analysis ({0:P2})" , InAnalysisFilesCount / (double) total )}
                };

                return itemRatingInfoList.ToArray();
            }
        }
        public ItemRatingInfo[] FileRatingValkyrie
        {
            get
            {
                var total = SafeFilesCount + UnknownFilesFound + MalwareFilesFound + NotAnalyzedFilesCount + PendingAnalysisFilesCount;

                var itemRatingInfoList = new List<ItemRatingInfo>
                {
                    new FileRatingValkyrieInfo { CloudVerdict = ValkyrieVerdict.Clean, Count = SafeFilesCount, Value = string.Format("Trusted: ({0:P2})", SafeFilesCount / (double) total)},
                    new FileRatingValkyrieInfo { CloudVerdict = ValkyrieVerdict.Undetected, Count = UnknownFilesFound, Value = string.Format("Unknown  ({0:P2})", UnknownFilesFound / (double) total)},
                    new FileRatingValkyrieInfo { CloudVerdict = ValkyrieVerdict.Malware, Count = MalwareFilesFound, Value = string.Format("Malicious ({0:P2})", MalwareFilesFound / (double) total)},
                    new FileRatingValkyrieInfo { CloudVerdict = ValkyrieVerdict.NotAnalyzed, Count = NotAnalyzedFilesCount, Value = string.Format("Lookup In Progress ({0:P2})" , NotAnalyzedFilesCount / (double) total)},
                    new FileRatingValkyrieInfo { CloudVerdict = ValkyrieVerdict.Pending, Count = PendingAnalysisFilesCount, Value = string.Format("In Analysis ({0:P2})" , PendingAnalysisFilesCount / (double) total )}
                };

                return itemRatingInfoList.ToArray();
            }
        }

        public ItemRatingInfo[] DevicesRating
        {
            get
            {
                int safe = 0;
                int vulnerable = 0;
                int infected = 0;
                int unknown = 0;
                //int notscanned = 0;
                var groupped = from target in _Targets group target by target.TargetVerdict into temp select temp;
                foreach (var item in groupped)
                {
                    if (item.Key == TargetVerdict.Safe)
                        safe = item.Count();
                    if (item.Key == TargetVerdict.Infected)
                        infected = item.Count();
                    if (item.Key == TargetVerdict.Vulnerable)
                        vulnerable = item.Count();
                    if (item.Key == TargetVerdict.Unknown)
                        unknown = item.Count();
                }

                int total = safe + vulnerable + infected + unknown;


                return new ItemRatingInfo[]
                {
                    new TargetRatingInfo(){TargetVerdict = TargetVerdict.Safe, Count = safe, Value = string.Format("Safe ({0:P2})", safe / (double) total)},
                    new TargetRatingInfo(){TargetVerdict = TargetVerdict.Infected, Count = infected, Value = string.Format("Infected ({0:P2})", infected / (double)total)},
                    new TargetRatingInfo(){TargetVerdict = TargetVerdict.Vulnerable, Count = vulnerable, Value = string.Format("At Risk ({0:P2})", vulnerable / (double) total)},
                    new TargetRatingInfo(){TargetVerdict = TargetVerdict.NotScanned, Count = unknown, Value = string.Format("Not Scanned ({0:P2})", unknown / (double) total)}
                };
            }
        }


        public ItemRatingInfo[] DevicesRatingValkyrie
        {
            get
            {
                IEnumerable<ScannedTarget> targets;
                IEnumerable<DetailedBadFile> files;
                MergeResults(_Targets, ValkyrieAnalysisResult, out targets, out files, out files);


                int safe = 0;
                int vulnerable = 0;
                int infected = 0;
                int unknown = 0;
                //int notscanned = 0;
                var groupped = from target in targets group target by target.TargetVerdict into temp select temp;
                foreach (var item in groupped)
                {
                    if (item.Key == TargetVerdict.Safe)
                        safe = item.Count();
                    if (item.Key == TargetVerdict.Infected)
                        infected = item.Count();
                    if (item.Key == TargetVerdict.Vulnerable)
                        vulnerable = item.Count();
                    if (item.Key == TargetVerdict.Unknown)
                        unknown = item.Count();
                }

                int total = safe + vulnerable + infected + unknown;

                return new ItemRatingInfo[]
                {
                    new TargetRatingInfo(){TargetVerdict = TargetVerdict.Safe, Count = safe, Value = string.Format("Safe ({0:P2})", safe / (double) total)},
                    new TargetRatingInfo(){TargetVerdict = TargetVerdict.Infected, Count = infected, Value = string.Format("Infected ({0:P2})", infected / (double)total)},
                    new TargetRatingInfo(){TargetVerdict = TargetVerdict.Vulnerable, Count = vulnerable, Value = string.Format("At Risk ({0:P2})", vulnerable / (double) total)},
                    new TargetRatingInfo(){TargetVerdict = TargetVerdict.NotScanned, Count = unknown, Value = string.Format("Not Scanned ({0:P2})", unknown / (double) total)}
                };
            }
        }


        public VulnerableDeviceInfoEx[] Top10VulnerableDevices
        {
            get
            {
                IEnumerable<ScannedTarget> targets;
                IEnumerable<DetailedBadFile> files;
                MergeResults(_Targets, ValkyrieAnalysisResult, out targets, out files, out files);

                var top10 = (from target in targets
                             where target.BadFilesCount > 0
                             orderby target.BadFilesCount descending
                             select target).Take(10);

                List<VulnerableDeviceInfoEx> devices = new List<VulnerableDeviceInfoEx>();
                foreach (var scanResult in top10)
                {
                    if (scanResult.UnknownFilesCount > 0)
                        devices.Add(new VulnerableDeviceInfoEx() { BadFilesCount = scanResult.UnknownFilesCount, BadFileType = "Unknown", Name = scanResult.HostName, Uid = scanResult.TargetUid });
                    if (scanResult.MalwareFilesCount > 0)
                        devices.Add(new VulnerableDeviceInfoEx() { BadFilesCount = scanResult.MalwareFilesCount, BadFileType = "Infected", Name = scanResult.HostName, Uid = scanResult.TargetUid });
                    if (scanResult.NotAnalyzedFilesCount > 0)
                        devices.Add(new VulnerableDeviceInfoEx() { BadFilesCount = scanResult.NotAnalyzedFilesCount, BadFileType = "Lookup in Progress", Name = scanResult.HostName, Uid = scanResult.TargetUid });
                    //if (scanResult.NoVerdictFilesCount > 0)
                    //    devices.Add(new VulnerableDeviceInfoEx() { BadFilesCount = scanResult.NoVerdictFilesCount, BadFileType = "No Verdict", Name = scanResult.HostName, Uid = scanResult.TargetUid });
                }

                return devices.ToArray();
            }
        }

        public VulnerableDeviceInfoEx[] Top10VulnerableDevicesValkyrie
        {
            get
            {
                IEnumerable<ScannedTarget> targets = new ScannedTarget[] { };
                IEnumerable<DetailedBadFile> files;
                MergeResults(_Targets, ValkyrieAnalysisResult, out targets, out files, out files);

                var top10 = (from target in targets
                             where target.BadFilesCount > 0
                             orderby target.BadFilesCount descending
                             select target).Take(10);

                List<VulnerableDeviceInfoEx> devices = new List<VulnerableDeviceInfoEx>();
                foreach (var scanResult in top10)
                {
                    if (scanResult.UnknownFilesCount > 0)
                        devices.Add(new VulnerableDeviceInfoEx() { BadFilesCount = scanResult.UnknownFilesCount, BadFileType = "Unknown", Name = scanResult.HostName, Uid = scanResult.TargetUid });
                    if (scanResult.MalwareFilesCount > 0)
                        devices.Add(new VulnerableDeviceInfoEx() { BadFilesCount = scanResult.MalwareFilesCount, BadFileType = "Infected", Name = scanResult.HostName, Uid = scanResult.TargetUid });
                    if (scanResult.NotAnalyzedFilesCount > 0)
                        devices.Add(new VulnerableDeviceInfoEx()
                        {
                            BadFilesCount = scanResult.NotAnalyzedFilesCount,
                            BadFileType = "Not Analyzed",
                            Name = scanResult.HostName,
                            Uid = scanResult.TargetUid
                        });
                    if (scanResult.PendingAnalysisFilesCount > 0)
                        devices.Add(new VulnerableDeviceInfoEx() { BadFilesCount = scanResult.PendingAnalysisFilesCount, BadFileType = "Pending", Name = scanResult.HostName, Uid = scanResult.TargetUid });
                }

                return devices.ToArray();
            }
        }
        public MostWantedBadFileInfo[] Top10PopularBadFiles
        {
            get
            {
                var allBadFiles = GetAllBadFilesSorted();
                return GetTop10PopularBadFiles(allBadFiles);
            }
        }

        public MostWantedBadFileInfo[] GetTop10PopularBadFiles(DetailedBadFile[] files)
        {
            var result = (from file in files
                          select
                              new MostWantedBadFileInfo()
                              {
                                  BadFileType = file.FileType,
                                  Count = file.InstancesTotalCount,
                                  Hash = file.Sha1,
                                  Name = file.Name
                              }).Take(10);
            return result.Reverse().ToArray();
        }


        public class MergedScanResults
        {
            public IEnumerable<ScannedTarget> MergedTargets { get; set; }
            public IEnumerable<DetailedBadFile> MergeUniqueResults { get; set; }
            public IEnumerable<DetailedBadFile> MergeNonUniqueResults { get; set; }
        }
        private void MergeResults(IEnumerable<IAptScanResult> flsResults, 
            Dictionary<string, FileResult> valkyrieResults, out IEnumerable<ScannedTarget> mergedTargets,
            out IEnumerable<DetailedBadFile> uniqueBadFiles, out IEnumerable<DetailedBadFile> nonUniqueBadFiles)
        {
            if (_mergedScanResultsCache != null)
            {
                mergedTargets = _mergedScanResultsCache.MergedTargets;
                uniqueBadFiles = _mergedScanResultsCache.MergeUniqueResults;
                nonUniqueBadFiles = _mergedScanResultsCache.MergeNonUniqueResults;
                return;
            }
            var statistics = _mergeHelper.MergeResults(flsResults, valkyrieResults, out mergedTargets, out uniqueBadFiles, out nonUniqueBadFiles);
            if (statistics != null) ReplaceStatistics(statistics);
            _mergedScanResultsCache = new MergedScanResults
            {
                MergeUniqueResults = uniqueBadFiles,
                MergeNonUniqueResults = nonUniqueBadFiles,
                MergedTargets = mergedTargets
            };
            
        }

        public DetailedBadFile[] AllBadFilesSorted
        {
            get; set;


        }

        public void UpdateAllBadFilesSorted()
        {
            AllBadFilesSorted = GetAllBadFilesSorted();
        }

        public DetailedBadFile[] GetAllBadFilesSorted(bool isUniqueSetOfFiles = true)
        {
            IEnumerable<ScannedTarget> targets;
            IEnumerable<DetailedBadFile> allUniqueBadFiles;
            IEnumerable<DetailedBadFile> allNonUniqueBadFiles;
            MergeResults(_Targets, ValkyrieAnalysisResult, out targets, out allUniqueBadFiles, out allNonUniqueBadFiles);

            //return (from file in globallyUniqueFiles.Values orderby file.Targets.Count descending select file).ToArray();
            return (isUniqueSetOfFiles ? allUniqueBadFiles : allNonUniqueBadFiles)
                .OrderByDescending(file => file.InstancesTotalCount).ThenBy(file => file.Name).ToArray();
        }

        public Dictionary<string, FileResult> ValkyrieAnalysisResult { get; set; }
        public Guid FlsLookupOperationUid { get; set; }
    }

    public class VulnerableDeviceInfo
    {
        public string Name { get; set; }
        public int BadFilesCount { get; set; }
    }

    public class VulnerableDeviceInfoEx
    {
        public Guid Uid { get; set; }
        public string Name { get; set; }
        public int BadFilesCount { get; set; }
        public string BadFileType { get; set; }
    }

    public class MostWantedBadFileInfo
    {
        public string Hash { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public BadFileType BadFileType { get; set; }
    }

    public enum BadFileType
    {
        Unknown = 0,
        Infected = 1,
        NotAnalyzed = 2,
        Pending = 3
    }
        
    public abstract class ItemRatingInfo
    {
        public string Value { get; set; }
        public int Count { get; set; }
    }

    public class TargetRatingInfo : ItemRatingInfo
    {
        public TargetVerdict TargetVerdict { get; set; }
    }

    public class FileRatingInfo : ItemRatingInfo
    {
        public CloudVerdict CloudVerdict { get; set; }
    }

    public class FileRatingValkyrieInfo : ItemRatingInfo
    {
        public ValkyrieVerdict CloudVerdict { get; set; }
    }
    public enum AptRiskAssessmentLevel
    {
        Ok = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Unknown = 4
    }

    public enum ScanResultType
    {
        NotPerformed,
        Completed,
        Failed,
        Canceled
    }

    public enum TargetVerdict
    {
        Unknown = 0,
        Safe = 1,
        Infected = 2,
        Vulnerable = 3,
        NotScanned = 4
    }
}
