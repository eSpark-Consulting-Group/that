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
        public AptReportData()
        {
            Targets = new IAptScanResult[] { };
        }
    
        private MergedScanResults _mergedScanResultsCache;
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

                return (int)((double)SafeFilesCount / total * 100);
            }
        }

        public string NetworkTrustLevelString
        {
            get { return NetworkTrustLevel >= 0 ? String.Format("{0} %", NetworkTrustLevel) : "Unknown"; }
        }

        public IAptScanResult[] Targets { get; set; }

        public IAptScanResult[] SucceededTargets
        {
            get { return (from t in Targets where t.ResultType == Core.ScanResultType.Completed select t).ToArray(); }
        }
        public IAptScanResult[] SucceededTargetsWithValkyrie
        {
            get
            {
                IEnumerable<ScannedTarget> targets;
                IEnumerable<DetailedBadFile> files;
                MergeResults(Targets, ValkyrieAnalysisResult, out targets, out files, out files);

                List<AptScanResult> scanResults = new List<AptScanResult>();
                foreach (var t in targets)
                {
                    var endPointFiles = (from file in files where file.Targets.Any(tg => tg.TargetUid == t.TargetUid) select file).ToArray();
                    var scanResult = new AptScanResult(t.Computer, Guid.Empty, null)
                    {
                        MalwareFilesCount = t.MalwareFilesCount,
                        UnknownFilesCount = t.UnknownFilesCount,
                        SafeFilesCount = t.SafeFilesCount,
                        AutorunsCount = t.AutorunsCount,
                        RunningProgramsCount = t.RunningProgramsCount,
                        ResultType = t.ResultType,
                        ResultErrorText = t.ResultErrorText,
                        TotalFilesCount = t.TotalFilesCount,
                        FailedToAnalyzeFilesCount = t.FailedToAnalyzeFilesCount,
                        NoVerdictFilesCount = t.NoVerdictFilesCount,
                        PendingAnalysisFilesCount = t.PendingAnalysisFilesCount,
                        UknownFilesValkyrie = endPointFiles.Where(f => f.FileType == BadFileType.Unknown).ToArray(),
                        MaliciousFilesValkyrie = endPointFiles.Where(f => f.FileType == BadFileType.Infected).ToArray(),
                        FailedToAnalyzeFilesValkyrie = endPointFiles.Where(f => f.FileType == BadFileType.NotAnalyzed).ToArray(),
                        PendingAnalysisFilesValkyrie = endPointFiles.Where(f => f.FileType == BadFileType.Pending).ToArray()
                    };

                    scanResults.Add(scanResult);
                }

                return scanResults.ToArray();
            }
        }

        private void CalculateTotal()
        {
            if (Targets == null) return;
            var statistics = StatisticsCalculator.CalculateFilesStatistics(Targets);

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

        public ItemRatingInfo[] FileRatingValkyrie
        {
            get
            {
                var total = SafeFilesCount + UnknownFilesFound + MalwareFilesFound + NotAnalyzedFilesCount +  PendingAnalysisFilesCount;
          
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
                var groupped = from target in Targets group target by target.TargetVerdict into temp select temp;
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
                MergeResults(Targets, ValkyrieAnalysisResult, out targets, out files, out files);


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
                MergeResults(Targets, ValkyrieAnalysisResult, out targets, out files, out files);

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
                MergeResults(Targets, ValkyrieAnalysisResult, out targets, out files, out files);

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

        private void MergeResults(IEnumerable<IAptScanResult> flsResults, Dictionary<string, FileResult> valkyrieResults, out IEnumerable<ScannedTarget> mergedTargets,
            out IEnumerable<DetailedBadFile> uniqueBadFiles, out IEnumerable<DetailedBadFile> nonUniqueBadFiles)
        {
            if (_mergedScanResultsCache != null)
            {
                mergedTargets = _mergedScanResultsCache.MergedTargets;
                uniqueBadFiles = _mergedScanResultsCache.MergeUniqueResults;
                nonUniqueBadFiles = _mergedScanResultsCache.MergeNonUniqueResults;
                return;
            }

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
                            if (module.Cloud == CloudVerdict.Malware)
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
                                        case CloudVerdict.Safe:
                                            mergedSafeCount++;
                                            break;
                                        case CloudVerdict.Unknown:
                                        case CloudVerdict.FirstSeen:
                                            mergedUnknownCount++;
                                            break;
                                        case CloudVerdict.Malware:
                                            mergedMalwareCount++;
                                            break;
                                        case CloudVerdict.FailedToAnalyze:
                                        case CloudVerdict.NoVerdict:
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
                            case CloudVerdict.Safe:
                                mergedSafeCount++;
                                break;
                            case CloudVerdict.Unknown:
                            case CloudVerdict.FirstSeen:
                                mergedUnknownCount++;
                                break;
                            case CloudVerdict.Malware:
                                mergedMalwareCount++;
                                break;
                            case CloudVerdict.FailedToAnalyze:
                                mergedErrorFilesCount++;
                                break;
                            case CloudVerdict.NoVerdict:
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
            ReplaceStatistics(statistics);

            _mergedScanResultsCache = new MergedScanResults
            {
                MergeUniqueResults = globalUniqueFiles.Values.ToArray(),
                MergeNonUniqueResults = nonUniqueBadFilesInternal,
                MergedTargets = mergedTargets
            };
            nonUniqueBadFiles = _mergedScanResultsCache.MergeNonUniqueResults;
            uniqueBadFiles = _mergedScanResultsCache.MergeUniqueResults;
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

        private static void AddBadFile(Dictionary<string, DetailedBadFile> uniqueFiles, DetailedBadFile badFile, IModule module)
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

        
        public DetailedBadFile[] AllBadFilesSorted
        {
            get;set;
            
        
        }

        public void UpdateAllBadFilesSorted()
        {
            AllBadFilesSorted = GetAllBadFilesSorted(false);
        }

        public DetailedBadFile[] GetAllBadFilesSorted(bool isUniqueSetOfFiles = true)
        {
            IEnumerable<ScannedTarget> targets;
            IEnumerable<DetailedBadFile> allUniqueBadFiles;
            IEnumerable<DetailedBadFile> allNonUniqueBadFiles;
            MergeResults(Targets, ValkyrieAnalysisResult, out targets, out allUniqueBadFiles, out allNonUniqueBadFiles);

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
