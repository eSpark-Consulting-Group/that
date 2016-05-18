using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using APTAT.Core.Valkyrie;
using Valkyrie.WebApiProvider.Enums;

namespace APTAT.Core.Reports
{
    public class AptReportData
    {
        private MergedScanResults _mergedScanResultsCache;
        private int _safeFilesCount = -1;
        private int _unknownFilesCount = -1;
        private int _malwareFilesCount = -1;

        public Guid OperationUid { get; set; } 
        public bool IsActiveDirectoryDevicesSource { get { return Targets.Any(t => !String.IsNullOrEmpty(t.OrganizationUnit)); } }
        private int _emptyFilesCount = -1;
        private int _skippedFilesCount = -1;
        private int _errorFileCount = -1;
        private int _serverErrorFileCount = -1;
        private int _totalFilesCound = -1;

        public DateTime ScanStarted { get; set; }
        public DateTime ScanFinished { get; set; }
        public int TotalTargetsCount { get; set; }
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

        public int ErrorFilesCount
        {
            get
            {
                if (_errorFileCount == -1)
                    CalculateTotal();
                return _errorFileCount;
            }
        }


        public int ServerErrorFilesCount
        {
            get
            {
                if (_serverErrorFileCount == -1)
                    CalculateTotal();
                return _serverErrorFileCount;
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
                if (UnknownFilesFound > 0)
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

        public ScanResult[] Targets { get; set; }

        public ScanResult[] SucceededTargets
        {
            get { return (from t in Targets where t.ResultType == Core.ScanResultType.Completed select t).ToArray(); }
        }
        public ScanResult[] SucceededTargetsWithValkyrie
        {
            get 
            {
                IEnumerable<ScannedTarget> targets;
                IEnumerable<DetailedBadFile> files;
                MergeResults(Targets, ValkyrieAnalysisResult, out targets, out files, out files);

                List<ScanResult> scanResults = new List<ScanResult>();
                foreach (var t in targets)
                {
                    var scanResult = new ScanResult()
                    {
                        MalwareFilesCount = t.MalwareFilesCount,
                        UnknownFilesCount = t.UnknownFilesCount,
                        SafeFilesCount = t.SafeFilesCount,
                        AutorunsCount = t.AutorunsCount,
                        RunningProgramsCount = t.RunningProgramsCount,
                        HostName = t.HostName,
                        IpAddress = t.IpAddress,
                        OrganizationUnit = t.OrganizationUnit,
                        ResultType = t.ResultType,
                        ResultErrorText = t.ResultErrorText,
                        TargetUid = t.TargetUid,
                        TotalFilesCount = t.TotalFilesCount,
                        ErrorFilesCount =  t.ErrorFilesCount,
                        ServerErrorFilesCount = t.ServerErrorFilesCount,
                    };

                    scanResult.SetAptReportInstance(t.AptReport);
                    scanResults.Add(scanResult);
                }

                return scanResults.ToArray();
            }
        }

        private void CalculateTotal()
        {
            var statistics = StatisticsCalculator.Calculate(Targets);

            ReplaceStatistics(statistics);
        }

        private void ReplaceStatistics(FilesStatistics statistics)
        {
            _safeFilesCount = statistics.SafeFilesCount;
            _unknownFilesCount = statistics.UnknownFilesCount;
            _malwareFilesCount = statistics.MalwareFilesCount;
            _emptyFilesCount = statistics.EmptyFilesCount;
            _skippedFilesCount = statistics.SkippedFilesCount;
            _errorFileCount = statistics.ErrorFilesCount;
            _serverErrorFileCount = statistics.ServerErrorFilesCount;
            _totalFilesCound = statistics.TotalFilesCound;
        }

        public ItemRatingInfo[] GetFileRatingCollection()
        {
            var itemRatingInfoList = new List<ItemRatingInfo>()
            {
                new FileRatingInfo(){CloudVerdict = CloudVerdict.Safe, Count = SafeFilesCount, Value = "Trusted Programs"},
                new FileRatingInfo(){CloudVerdict = CloudVerdict.Unknown, Count = UnknownFilesFound, Value = "Unknown Programs"},
                //**
                new FileRatingInfo() { CloudVerdict = CloudVerdict.Malware, Count = MalwareFilesFound, Value = "Malwares" }
            };

            return itemRatingInfoList.ToArray();
        }

        public ItemRatingInfo[] GetDevicesRatingCollection()
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

            return new ItemRatingInfo[]
            {
                new TargetRatingInfo(){TargetVerdict = TargetVerdict.Safe, Count = safe, Value = "Safe Devices"},
                new TargetRatingInfo(){TargetVerdict = TargetVerdict.Vulnerable, Count = vulnerable, Value = "Devices At Risk"},
                new TargetRatingInfo(){TargetVerdict = TargetVerdict.Infected, Count = infected, Value = "Infected Devices"},
                new TargetRatingInfo(){TargetVerdict = TargetVerdict.Unknown, Count = unknown, Value = "Failed/Canceled Devices"},
            };
        }

        public VulnerableDeviceInfoEx[] GetTop10VulnerableDevices()
        {
            IEnumerable<ScannedTarget> targets = new ScannedTarget[] {};
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
                    devices.Add(new VulnerableDeviceInfoEx() { BadFilesCount = scanResult.UnknownFilesCount, BadFileType = BadFileType.Unknown, Name = scanResult.HostName, Uid = scanResult.TargetUid });
                if (scanResult.MalwareFilesCount > 0)
                    devices.Add(new VulnerableDeviceInfoEx() { BadFilesCount = scanResult.MalwareFilesCount, BadFileType = BadFileType.Infected, Name = scanResult.HostName, Uid = scanResult.TargetUid });
            }

            return devices.ToArray();
       }

        public MostWantedBadFileInfo[] GetTop10PopularBadFiles()
        {
            var allBadFiles = GetAllBadFilesSorted();
            return GetTop10PopularBadFiles(allBadFiles);
        }

        public MostWantedBadFileInfo[] GetTop10PopularBadFiles(DetailedBadFile[] files)
        {
            var result = (from file in files
                          select
                              new MostWantedBadFileInfo()
                              {
                                  BadFileType = file.FileType,
                                  Count = file.Targets.Count,
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

        private void MergeResults(IEnumerable<ScanResult> flsResults, IEnumerable<FileResult> valkyrieResults, out IEnumerable<ScannedTarget> mergedTargets,
            out IEnumerable<DetailedBadFile> uniqueBadFiles, out IEnumerable<DetailedBadFile> nonUniqueBadFiles)
        {
            if (_mergedScanResultsCache != null)
            {
                mergedTargets = _mergedScanResultsCache.MergedTargets;
                uniqueBadFiles = _mergedScanResultsCache.MergeUniqueResults;
                nonUniqueBadFiles = _mergedScanResultsCache.MergeNonUniqueResults;
                return;
            }

            int totalFilesCound = 0;
            int safeFilesCount = 0;
            int unknownFilesCount = 0;
            int malwareFilesCount = 0;
            int emptyFilesCount = 0;
            int skippedFilesCount = 0;
            int errorFilesCount = 0;
            int serverErrorFilesCount = 0;
            var targets = new List<ScannedTarget>();

            List<DetailedBadFile> nonUniqueBadFilesInternal = new List<DetailedBadFile>();
            var fileResults = (valkyrieResults??new FileResult[0]) as FileResult[] ?? valkyrieResults.ToArray();
            Dictionary<string, DetailedBadFile> globalUniqueFiles = new Dictionary<string, DetailedBadFile>();
            foreach (var flsResult in flsResults)
            {
                if (flsResult.ResultType != Core.ScanResultType.Completed)
                    continue;
                Dictionary<string, DetailedBadFile> uniqueFiles = new Dictionary<string, DetailedBadFile>();

                int mergedSafeCount = 0;
                int mergedUnknownCount = 0;
                int mergedMalwareCount = 0;

                #region Working with a single scan result

                foreach (var module in flsResult.Files)
                {
                    var fileResult = fileResults.FirstOrDefault(v => v.ValkyrieAnalysisResult != null && v.ValkyrieAnalysisResult.SHA1 != null && v.ValkyrieAnalysisResult.SHA1.ToUpper() == module.Sha1.ToUpper());
                    if (fileResult != null)
                    {
                        DetailedBadFile badFile = new DetailedBadFile(module, new ScannedTarget(flsResult));

                        badFile.SetValkyrieAnalysis(fileResult.ValkyrieAnalysisResult);
                        if (badFile.Verdict != ValkyrieVerdict.Clean)
                        {
                            AddBadFile(uniqueFiles, badFile, module);
                        }
                        nonUniqueBadFilesInternal.Add(badFile);
                        #region Calculate Statistics

                        switch (fileResult.ValkyrieAnalysisResult.Verdict)
                        {
                            case ValkyrieVerdict.Clean:
                                mergedSafeCount++;
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
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        #endregion
                    }
                    else
                    {
                        if (module.IsBad)
                        {
                            DetailedBadFile badFile = new DetailedBadFile(module, new ScannedTarget(flsResult));
                            AddBadFile(uniqueFiles, badFile, module);
                            nonUniqueBadFilesInternal.Add(badFile);
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
                    UnknownFilesCount = mergedUnknownCount
                };
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
                }

                safeFilesCount += mergedSafeCount;
                unknownFilesCount += mergedUnknownCount;
                malwareFilesCount += mergedMalwareCount;
                errorFilesCount += flsResult.ErrorFilesCount;
                serverErrorFilesCount += flsResult.ServerErrorFilesCount;
                totalFilesCound += flsResult.TotalFilesCount;
                emptyFilesCount += flsResult.EmptyFilesCount;
                skippedFilesCount += flsResult.SkippedFilesCount;
            }

            mergedTargets = targets;

            var statistics = new FilesStatistics()
            {
                EmptyFilesCount = emptyFilesCount,
                MalwareFilesCount = malwareFilesCount,
                SafeFilesCount = safeFilesCount,
                TotalFilesCound = totalFilesCound,
                UnknownFilesCount = unknownFilesCount,
                SkippedFilesCount = skippedFilesCount,
                ErrorFilesCount =  errorFilesCount,
                ServerErrorFilesCount = serverErrorFilesCount
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

        private static void AddBadFile(Dictionary<string, DetailedBadFile> uniqueFiles, DetailedBadFile badFile, Module module)
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

        public DetailedBadFile[] GetAllBadFilesSorted(bool isUniqueSetOfFiles = true)
        {
            IEnumerable<ScannedTarget> targets;
            IEnumerable<DetailedBadFile> allUniqueBadFiles;
            IEnumerable<DetailedBadFile> allNonUniqueBadFiles;
            MergeResults(Targets, ValkyrieAnalysisResult, out targets, out allUniqueBadFiles, out allNonUniqueBadFiles);

            //return (from file in globallyUniqueFiles.Values orderby file.Targets.Count descending select file).ToArray();
            return (isUniqueSetOfFiles? allUniqueBadFiles: allNonUniqueBadFiles).OrderByDescending(file => file.Targets.Count).ThenBy(file => file.Name).ToArray();
        }

        public List<FileResult> ValkyrieAnalysisResult { get; set; }
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
        public BadFileType BadFileType { get; set; }
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
        Infected = 1
    }

    public class ScannedTarget
    {
        private readonly List<DetailedBadFile.BadFileInstanceInfo> _badFileInstances = new List<DetailedBadFile.BadFileInstanceInfo>();

        public ScannedTarget()
        {
        }

        public ScannedTarget(ScanResult scanResult)
        {
            if (scanResult != null)
            {
                TargetUid = scanResult.TargetUid;
                HostName = scanResult.HostName;
                IpAddress = scanResult.IpAddress;
                OsName = scanResult.OsName;
                OrganizationUnit = scanResult.OrganizationUnit;
                TotalFilesCount = scanResult.TotalFilesCount;
                UnknownFilesCount = scanResult.UnknownFilesCount;
                ErrorFilesCount = scanResult.ErrorFilesCount;
                ServerErrorFilesCount = scanResult.ServerErrorFilesCount;
                SafeFilesCount = scanResult.SafeFilesCount;
                MalwareFilesCount = scanResult.MalwareFilesCount;
                RunningProgramsCount = scanResult.RunningProgramsCount;
                AutorunsCount = scanResult.AutorunsCount;
                TargetVerdictString = scanResult.TargetVerdictString;
                AptReport = scanResult.AptReport;
                ResultErrorText = scanResult.ResultErrorText;
                ResultType = scanResult.ResultType;
            }
        }

        public AptReport AptReport { get; set; }

        public string ResultErrorText { get; set; }
        public Core.ScanResultType ResultType { get; set; }
        public Guid TargetUid { get; set; }
        public string HostName { get; set; }
        public string IpAddress { get; set; }
        public string OsName { get; set; }
        public string OrganizationUnit { get; set; }
        public int TotalFilesCount { get; set; }
        public int UnknownFilesCount { get; set; }
        public int SafeFilesCount { get; set; }
        public int MalwareFilesCount { get; set; }
        public int ErrorFilesCount { get; set; }
        public int ServerErrorFilesCount { get; set; }
        public int RunningProgramsCount { get; set; }

        public string TargetVerdictString { get; set; }

        public int AutorunsCount { get; set; }

        public int InstancesCount
        {
            get { return BadFileInstances.Count; }
        }

        public int BadFilesCount
        {
            get { return UnknownFilesCount + MalwareFilesCount; }
        }

        public string Instances
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var instance in BadFileInstances)
                {
                    string str = String.Format("{0}\n", instance.Path);
                    sb.AppendLine(str);
                }
                return sb.ToString();
            }
        }

        public List<DetailedBadFile.BadFileInstanceInfo> BadFileInstances
        {
            get { return _badFileInstances; }
        }
    }

    public class DetailedBadFile
    {
        private readonly List<ScannedTarget> _targets = new List<ScannedTarget>();

        public DetailedBadFile()
        {
        }

        public DetailedBadFile(Module module, ScannedTarget target)
        {
            Verdict = ValkyrieVerdict.NotAnalyzed;
            if (module != null)
            {
                Sha1 = module.Sha1;
                Name = System.IO.Path.GetFileName(module.Path);
                Path = module.Path;
                Size = module.Size;
                Signer = module.Signer;
                Version = module.Version;
                Copyright = module.Copyright;
                Product = module.Product;
                Cloud = module.Cloud;
                FileSizeFormatted = module.FileSizeFormatted;
                Created = module.Created;
                Accessed = module.Accessed;
                Modified = module.Modified;
                FileAge = module.FileAge;
            }

            target.BadFileInstances.Add(new BadFileInstanceInfo(module));
            _targets.Add(target);
        }
        public string Path { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Accessed { get; set; }

        public string Sha1 { get; set; }
        public string Name { get; set; }

        public long Size { get; set; }
        public string Signer { get; set; }
        public string Version { get; set; }
        public string Copyright { get; set; }
        public string Product { get; set; }
        public CloudVerdict Cloud { get; set; }
        public string FileSizeFormatted { get; set; }
        
        public BadFileType FileType
        {
            get
            {
                if (Verdict == ValkyrieVerdict.NotAnalyzed)
                    return Cloud == CloudVerdict.Unknown || Cloud == CloudVerdict.FirstSeen ? BadFileType.Unknown : BadFileType.Infected;
                return Verdict == ValkyrieVerdict.Undetected ? BadFileType.Unknown : BadFileType.Infected;
            }
        }

        public void SetValkyrieAnalysis(ValkyrieAnalysisResult analysisResult)
        {
            if (analysisResult == null)
                return;
            IsWhiteListed = analysisResult.IsWhiteListed;
            MalwareProbability = analysisResult.MalwareProbability;
            ReturnCode = analysisResult.ReturnCode;
            Verdict = analysisResult.Verdict;
            IsWhiteListed = analysisResult.IsWhiteListed;
            IsSkipped = analysisResult.IsSkipped;
            SkipReason = analysisResult.SkipReason;
            OperationStatus = analysisResult.OperationStatus;
        }
        


        public int TargetsCount
        {
            get { return Targets.Count; }
        }


        public string CloudString
        {
            get
            {
                switch (Cloud)
                {
                    case CloudVerdict.Safe:
                        return "Trusted";
                    case CloudVerdict.Unknown:
                        return "Unknown";
                    case CloudVerdict.Malware:
                        return "Malware";
                    case CloudVerdict.FirstSeen:
                        return "Unknown (FirstSeen)";
                    case CloudVerdict.FailedToAnalyze:
                        return "Error";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                ;
            }
        }

        public List<ScannedTarget> Targets
        {
            get { return _targets; }
        }

        public class BadFileInstanceInfo
        {
            public BadFileInstanceInfo()
            {
            }

            public BadFileInstanceInfo(Module module)
            {
                if (module != null)
                {
                    Id = module.Id;
                    Path = module.Path;
                    Created = module.Created;
                    Modified = module.Modified;
                    Accessed = module.Accessed;
                }
            }

            public string Id { get; set; }
            public string Path { get; set; }
            public DateTime Created { get; set; }
            public DateTime Modified { get; set; }
            public DateTime Accessed { get; set; }
        }

        public bool IsWhiteListed { get; set; }

        public string WhiteListed
        {
            get { return IsWhiteListed ? "Whitelisted" : "Not whitelisted"; }
        }

        public float MalwareProbability { get; set; }

        public string MalwareProbabilityFormatted
        {
            get { return string.Format("{0:P}", MalwareProbability/100); }
        }

        public ErrorCodesEnum ReturnCode { get; set; }

        public ValkyrieVerdict Verdict { get; set; }

        public string ValkyrieVerdictString
        {
            get
            {
                string result = this.Verdict.GetEnumDescription();
                if (this.IsSkipped)
                {
                    if (!String.IsNullOrWhiteSpace(this.OperationStatus))
                    {
                        if (OperationStatus.Length > 80)
                        {
                            var s = OperationStatus.Substring(0, 80);
                            s = s.Substring(0, s.LastIndexOf(" "));
                            return result += " (" + s + ")";
                        }
                        result += " (" + this.OperationStatus + ")";
                    }
                }

                return result;
            }
        }

        public bool IsSkipped { get; set; }

        public string SkipReason { get; set; }

        public string OperationStatus { get; set; }

        public string FileAge { get; set; }
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
