using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Directory;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;


namespace APTAT.Core.Reports
{
    public partial class AptReportData
    {

        public AptReportData(IReportData reportData)
        {
            _Targets = new IAptScanResult[] { };
            _mergeHelper = new ResultMergeHelper();

            OperationUid = reportData.OperationUid;
            ScanStarted = reportData.OperationContext.StartedTime.ToLocalTime();
            ScanFinished = reportData.OperationContext.CompletedTime.GetValueOrDefault().ToLocalTime();
            TargetsFailed = reportData.OperationContext.FailedTargetsCount;
            TargetsNotPerformed = reportData.OperationContext.NotPerformedTargetsCount;
            TotalTargetsCount = reportData.OperationContext.TotalTargetsCount;
            TargetsSucceeded = reportData.OperationContext.SucceededTargetsCount;
            DevicesSourceDescription = reportData.OperationContext.DeviceSourceDescription;
            SetTargets(reportData.OperationContext.ScanResults.ToArray());

        }

        private readonly ResultMergeHelper _mergeHelper;
        private int _safeFilesCount = -1;
        private int _unknownFilesCount = -1;
        private int _malwareFilesCount = -1;

        public string OperationUid { get; set; }
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
                    case ETargetsDiscovererType.AutoDiscovery:
                        return $"Auto Discovery ({DevicesSourceDescription.Description})";

                    default:
                        return string.Empty;
                }
            }
        }
        public bool IsActiveDirectoryDevicesSource => DevicesSourceDescription.Type == ETargetsDiscovererType.ActiveDirectory;

        public int TargetsNotPerformed { get; set; }
        public int TargetsScanned => TotalTargetsCount - TargetsNotPerformed;
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

        public string NetworkTrustLevelString => NetworkTrustLevel >= 0 ? $"{NetworkTrustLevel} %" : "Unknown";

        private void SetTargets(IAptScanResult[] targets)
        {
            _Targets = targets;
        }

        protected IAptScanResult[] _Targets;

        public virtual ScanResult[] SucceededTargets
        {
            get
            {
                var targets = (from t in _Targets where t.ResultType == Core.ScanResultType.Completed select t).ToArray();
                List<ScanResult> scanResults = new List<ScanResult>();
                foreach (var t in targets)
                {
                    ScanResultType resultType = t.ResultType.ToBusiness();
                    DetailedBadFile[] scanModules = t.Files.MapToBusiness(new ScannedTarget(t));
                    var scanResult = new ScanResult
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
                        MalwareFiles = scanModules.Where(m => m.Cloud == CloudVerdict.Malware).ToArray(),
                        UnknownFiles = scanModules.Where(m => m.Cloud.IsUnknown()).ToArray(),
                        LookupInProgressFiles = scanModules.Where(m => m.Cloud.IsLookupInProgress()).ToArray()
                    };

                    scanResults.Add(scanResult);
                }
                return scanResults.ToArray();
            }
        }

        public  virtual void CalculateTotal()
        {
            if (_Targets == null) return;
            var statistics = StatisticsCalculator.CalculateFilesStatistics(_Targets);
           ReplaceStatistics(statistics);
        }

        protected void ReplaceStatistics(IFilesStatistics statistics)
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

        public AptReportData()
        {

        }



        #region Charts data
        public virtual ItemRatingInfo[] FileRating
        {
            get
            {
                var total = SafeFilesCount + UnknownFilesFound + MalwareFilesFound + LookupInProgressFilesCount;

                var itemRatingInfoList = new List<ItemRatingInfo>
                {
                    new FileRatingInfo
                    {CloudVerdict = CloudVerdict.Safe, Count = SafeFilesCount, Value =
                        $"Trusted ({SafeFilesCount / (double) total:P2})"
                    },
                    new FileRatingInfo
                    {
                        CloudVerdict = CloudVerdict.Unknown, Count = UnknownFilesFound, Value = $"Unknown  ({UnknownFilesFound / (double) total:P2})"
                    },
                    new FileRatingInfo
                    {
                        CloudVerdict = CloudVerdict.Malware, Count = MalwareFilesFound, Value = $"Malicious ({MalwareFilesFound / (double) total:P2})"
                    },
                    new FileRatingInfo
                    {
                        CloudVerdict = CloudVerdict.FailedToAnalyze, Count = LookupInProgressFilesCount, Value =$"Lookup In Progress ({LookupInProgressFilesCount / (double) total:P2})"
                    }
                    //new FileRatingInfo() { CloudVerdict = CloudVerdict.NoVerdict, Count = NoVerdictFilesCount, Value = "No Verdict " }
                };

                return itemRatingInfoList.ToArray();
            }
        }

        public virtual ItemRatingInfo[] DevicesRating => CalculateTargetVerdict(_Targets);

        public virtual VulnerableDeviceInfoEx[] Top10VulnerableDevices
        {
            get
            {
                var top10 = (from target in _Targets
                             where target.BadFilesCount > 0
                             orderby target.BadFilesCount descending
                             select target).Take(10);

                List<VulnerableDeviceInfoEx> devices = new List<VulnerableDeviceInfoEx>();
                foreach (var scanResult in top10)
                {
                    if (scanResult.UnknownFilesCount > 0)
                        devices.Add(new VulnerableDeviceInfoEx { BadFilesCount = scanResult.UnknownFilesCount, BadFileType = "Unknown", Name = scanResult.Computer.Name, Uid = scanResult.TargetUid });
                    if (scanResult.MalwareFilesCount > 0)
                        devices.Add(new VulnerableDeviceInfoEx { BadFilesCount = scanResult.MalwareFilesCount, BadFileType = "Infected", Name = scanResult.Computer.Name, Uid = scanResult.TargetUid });
                    if (scanResult.LookupInProgressFilesCount > 0)
                        devices.Add(new VulnerableDeviceInfoEx { BadFilesCount = scanResult.LookupInProgressFilesCount, BadFileType = "Lookup in Progress", Name = scanResult.Computer.Name, Uid = scanResult.TargetUid });
                }

                return devices.ToArray();
            }
        }

        public MostWantedBadFileInfo[] Top10PopularBadFiles
        {
            get
            {
                return GetTop10PopularBadFiles(AllBadFilesSorted);
            }
        }

        #endregion

        protected ItemRatingInfo[] CalculateTargetVerdict(IEnumerable<ITargetVerdicted> targets)
        {
            int safe = 0;
            int vulnerable = 0;
            int infected = 0;
            int unknown = 0;
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
                    new TargetRatingInfo
                    {TargetVerdict = TargetVerdict.Safe, Count = safe, Value =
                        $"Safe ({safe / (double) total:P2})"
                    },
                    new TargetRatingInfo
                    {TargetVerdict = TargetVerdict.Infected, Count = infected, Value =
                        $"Infected ({infected / (double) total:P2})"
                    },
                    new TargetRatingInfo
                    {TargetVerdict = TargetVerdict.Vulnerable, Count = vulnerable, Value =
                        $"At Risk ({vulnerable / (double) total:P2})"
                    },
                    new TargetRatingInfo
                    {TargetVerdict = TargetVerdict.NotScanned, Count = unknown, Value =
                        $"Not Scanned ({unknown / (double) total:P2})"
                    }
            };
        }

        private MergedScanResults<DetailedBadFile> _mergedScanResultsCache;
        private DetailedBadFile[] _allBadFilesSorted;


        private void MergeResults(IEnumerable<IAptScanResult> flsResults,
         out IEnumerable<ScannedTarget> mergedTargets,
         out IEnumerable<DetailedBadFile> uniqueBadFiles, out IEnumerable<DetailedBadFile> nonUniqueBadFiles)
        {
            if (_mergedScanResultsCache != null)
            {
                mergedTargets = _mergedScanResultsCache.MergedTargets;
                uniqueBadFiles = _mergedScanResultsCache.MergeUniqueResults;
                nonUniqueBadFiles = _mergedScanResultsCache.MergeNonUniqueResults;
                return;
            }
            var statistics = _mergeHelper.MergeResults(flsResults, out mergedTargets, out uniqueBadFiles, out nonUniqueBadFiles);
            if (statistics != null) ReplaceStatistics(statistics);
            _mergedScanResultsCache = new MergedScanResults<DetailedBadFile>
            {
                MergeUniqueResults = uniqueBadFiles,
                MergeNonUniqueResults = nonUniqueBadFiles,
                MergedTargets = mergedTargets
            };

        }


        public DetailedBadFile[] AllBadFilesSorted
        {
            get
            {
                if (_allBadFilesSorted == null)
                {
                    _allBadFilesSorted = GetAllBadFilesSorted();
                }

                return _allBadFilesSorted;
            }

        }

        //protected virtual DetailedBadFile[] GetAllBadFiles()
        //{
        //    List<DetailedBadFile> files = new List<DetailedBadFile>();

        //    foreach (var scanResult in _Targets)
        //    {
        //        var badFilesFromTarget = scanResult.GetUniqueBadFiles();
        //        files.AddRange(badFilesFromTarget);
        //    }

        //    return files.ToArray();
        //}
        protected  virtual DetailedBadFile[] GetAllBadFilesSorted(bool isUniqueSetOfFiles = true)
        {
            IEnumerable<ScannedTarget> targets;
            IEnumerable<DetailedBadFile> allUniqueBadFiles;
            IEnumerable<DetailedBadFile> allNonUniqueBadFiles;
            if (_Targets == null) throw new InvalidOperationException("Targets required");
            MergeResults(_Targets, out targets, out allUniqueBadFiles, out allNonUniqueBadFiles);

            //return (from file in globallyUniqueFiles.Values orderby file.Targets.Count descending select file).ToArray();
            return (isUniqueSetOfFiles ? allUniqueBadFiles : allNonUniqueBadFiles)
                .OrderByDescending(file => file.InstancesTotalCount).ThenBy(file => file.Name).ToArray();
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


        public Guid FlsLookupOperationUid { get; set; }

    }
}
