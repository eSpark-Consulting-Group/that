using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace APTAT.Core.Reports
{
    public class AptReportData
    {
        private int _totalFilesCound = -1;
        private int _safeFilesCount = -1;
        private int _unknownFilesCount = -1;
        private int _malwareFilesCount = -1;

        public string TargetsSource { get; set; }

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
                if(_safeFilesCount == -1)
                    CalculateTotal();
                return _safeFilesCount;
            }
        }

        public int UnknownFilesFound
        {
            get
            {
                if(_unknownFilesCount == -1)
                    CalculateTotal();
                return _unknownFilesCount;
            }
        }

        public int MalwareFilesFound
        {
            get
            {
                if(_malwareFilesCount == -1)
                    CalculateTotal();
                return _malwareFilesCount;
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
                return (int)((double) SafeFilesCount/TotalFilesScanned * 100);
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

        private void CalculateTotal()
        {
            _totalFilesCound = 0;
            _safeFilesCount = 0;
            _unknownFilesCount = 0;
            _malwareFilesCount = 0;
            
            foreach (var scanResult in Targets)
            {
                if (scanResult.ResultType == Core.ScanResultType.Completed)
                {
                    _totalFilesCound += scanResult.TotalFilesCount;
                    _safeFilesCount += scanResult.SafeFilesCount;
                    _unknownFilesCount += scanResult.UnknownFilesCount;
                    _malwareFilesCount += scanResult.MalwareFilesCount;
                }
            }
        }

        public ItemRatingInfo[] GetFileRatingCollection()
        {
            return new ItemRatingInfo[]
            {
                new FileRatingInfo(){CloudVerdict = CloudVerdict.Safe, Count = SafeFilesCount, Value = "Trusted Programs"},
                new FileRatingInfo(){CloudVerdict = CloudVerdict.Unknown, Count = UnknownFilesFound, Value = "Unknown Programs"},
                new FileRatingInfo(){CloudVerdict = CloudVerdict.Malware, Count = MalwareFilesFound, Value = "Malwares"}
            };
        }

        public ItemRatingInfo[] GetDevicesRatingCollection()
        {
            int safe = 0;
            int vulnerable = 0;
            int infected = 0;
            int unknown = 0;
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
            var top10 = (from target in SucceededTargets
                        where target.BadFilesCount > 0
                        orderby target.BadFilesCount
                        select target).Take(10);

            List<VulnerableDeviceInfoEx> devices = new List<VulnerableDeviceInfoEx>();
            foreach (var scanResult in top10)
            {
                if(scanResult.UnknownFilesCount > 0)
                    devices.Add(new VulnerableDeviceInfoEx(){BadFilesCount = scanResult.UnknownFilesCount, BadFileType = BadFileType.Unknown, Name = scanResult.HostName, Uid = scanResult.TargetUid});
                if(scanResult.MalwareFilesCount > 0)
                    devices.Add(new VulnerableDeviceInfoEx(){BadFilesCount = scanResult.MalwareFilesCount, BadFileType = BadFileType.Infected, Name = scanResult.HostName, Uid = scanResult.TargetUid});
            }

            return devices.ToArray();

            //return (from target in Targets
            //             where target.BadFilesCount > 0 orderby target.BadFilesCount
            //             select
            //                 new VulnerableDeviceInfoEx() { BadFilesCount = target, Name = target.HostName, BadFileType =  }).Take(10).ToArray();
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
                                  Hash = file.Hash,
                                  Name = file.Name
                              }).Take(10);
            return result.ToArray();
        }

        private DetailedBadFile[] GetAllBadFiles()
        {
            List<DetailedBadFile> files = new List<DetailedBadFile>();

            foreach (var scanResult in Targets)
            {
                var badFilesFromTarget = scanResult.GetUniqueBadFiles();
                files.AddRange(badFilesFromTarget);
            }

            return files.ToArray();
        }

        public DetailedBadFile[] GetAllBadFilesSorted()
        {
            Dictionary<string, DetailedBadFile> globallyUniqueFiles = new Dictionary<string, DetailedBadFile>(); 
            
            DetailedBadFile[] allBadFiles = GetAllBadFiles();
            foreach (var badFile in allBadFiles)
            {
                if(globallyUniqueFiles.ContainsKey(badFile.Hash))
                {
                    globallyUniqueFiles[badFile.Hash].Targets.AddRange(badFile.Targets);
                }
                else
                {
                    globallyUniqueFiles.Add(badFile.Hash, badFile);
                }
            }

            return (from file in globallyUniqueFiles.Values orderby file.Targets.Count descending select file).ToArray();
        }
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
            if(scanResult != null)
            {
                TargetUid = scanResult.TargetUid;
                HostName = scanResult.HostName;
                IpAddress = scanResult.IpAddress;
                OsName = scanResult.OsName;
                OrganizationUnit = scanResult.OrganizationUnit;
                TotalFilesCount = scanResult.TotalFilesCount;
                UnknownFilesCount = scanResult.UnknownFilesCount;
                SafeFilesCount = scanResult.SafeFilesCount;
                MalwareFilesCount = scanResult.MalwareFilesCount;
            }
        }

        public Guid TargetUid { get; set; }
        public string HostName { get; set; }
        public string IpAddress { get; set; }
        public string OsName { get; set; }
        public string OrganizationUnit { get; set; }
        public int TotalFilesCount { get; set; }
        public int UnknownFilesCount { get; set; }
        public int SafeFilesCount { get; set; }
        public int MalwareFilesCount { get; set; }
        
        public int InstancesCount
        {
            get { return BadFileInstances.Count; }
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
            if (module != null)
            {
                Hash = module.Sha1;
                Name = Path.GetFileName(module.Path);
                Size = module.Size;
                Signer = module.Signer;
                Version = module.Version;
                Copyright = module.Copyright;
                Product = module.Product;
                Cloud = module.Cloud;
                FileSizeFormatted = module.FileSizeFormatted;
                FileType = module.Cloud == CloudVerdict.Unknown ? BadFileType.Unknown : BadFileType.Infected;
            }

            target.BadFileInstances.Add(new BadFileInstanceInfo(module));
            _targets.Add(target);
        }

        public string Hash { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string Signer { get; set; }
        public string Version { get; set; }
        public string Copyright { get; set; }
        public string Product { get; set; }
        public CloudVerdict Cloud { get; set; }
        public string FileSizeFormatted { get; set; }
        public BadFileType FileType { get; set; }
        public int TargetsCount
        {
            get { return Targets.Count; }
        }

        public string CloudString
        {
            get { return Cloud.ToString(); }
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
                if(module != null)
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
        Vulnerable = 3
    }
}
