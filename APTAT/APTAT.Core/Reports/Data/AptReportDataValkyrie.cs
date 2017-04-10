using System.Collections.Generic;
using System.Linq;
namespace APTAT.Core.Reports
{
    public class AptReportDataValkyrie : AptReportData
    {
        public AptReportDataValkyrie(IResultMergeHelper mergeHelper, ReportData reportData) : base(mergeHelper, reportData)
        {

        }

        public override ItemRatingInfo[] DevicesRating
        {
            get
            {
                IEnumerable<ScannedTarget> targets;
                IEnumerable<DetailedBadFile> files;
                MergeResults(_Targets, ValkyrieAnalysisResult, out targets, out files, out files);
                return CalculateTargetVerdict(targets);
            }
        }
        public override VulnerableDeviceInfoEx[] Top10VulnerableDevices
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
                    new FileRatingValkyrieInfo { CloudVerdict = APTAT.Valkyrie.ValkyrieVerdict.Clean, Count = SafeFilesCount, Value =
                        $"Trusted ({SafeFilesCount / (double) total:P2})"
                    },
                    new FileRatingValkyrieInfo { CloudVerdict = APTAT.Valkyrie.ValkyrieVerdict.Undetected, Count = UnknownFilesFound, Value =
                        $"Unknown  ({UnknownFilesFound / (double) total:P2})"
                    },
                    new FileRatingValkyrieInfo { CloudVerdict = APTAT.Valkyrie.ValkyrieVerdict.Malware, Count = MalwareFilesFound, Value =
                        $"Malicious ({MalwareFilesFound / (double) total:P2})"
                    },
                    new FileRatingValkyrieInfo { CloudVerdict = APTAT.Valkyrie.ValkyrieVerdict.NotAnalyzed, Count = NotAnalyzedFilesCount, Value =
                        $"Lookup In Progress ({NotAnalyzedFilesCount / (double) total:P2})"
                    },
                    new FileRatingValkyrieInfo { CloudVerdict = APTAT.Valkyrie.ValkyrieVerdict.Pending, Count = PendingAnalysisFilesCount, Value =
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
                IEnumerable<DetailedBadFile> files;
                MergeResults(_Targets, ValkyrieAnalysisResult, out targets, out files, out files);
                var detailedBadFiles = files as DetailedBadFile[] ?? files.ToArray();

                List<ScanResultValkyrie> scanResults = new List<ScanResultValkyrie>();
                foreach (var t in targets)
                {
                    var endPointFiles = (from file in detailedBadFiles where file.Targets.Any(tg => tg.TargetUid == t.TargetUid) select file).ToArray();
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
                        PendingAnalysisFilesValkyrie = endPointFiles.Where(f => f.FileType == BadFileType.Pending).ToArray()
                    };

                    scanResults.Add(scanResult);
                }
                var result = scanResults.OrderBy(t => t.SeverityIndex).ToArray();
                return result;
            }
        }

    }
}
