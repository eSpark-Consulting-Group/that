using System.Collections.Generic;
using System.Linq;
namespace APTAT.Core.Reports
{
    public class AptReportDataFAT : AptReportDataValkyrie
    {
        public AptReportDataFAT(IResultMergeHelper mergeHelper, ReportData reportData) : base(mergeHelper, reportData)
        {
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

                if (top10.All(t => (t.UnknownFilesCount + t.MalwareFilesCount + t.NotAnalyzedFilesCount + t.PendingAnalysisFilesCount) == 0)) return new VulnerableDeviceInfoEx[0];
                List<VulnerableDeviceInfoEx> devices = new List<VulnerableDeviceInfoEx>();
                foreach (var scanResult in top10)
                {

                    devices.Add(new VulnerableDeviceInfoEx() { BadFilesCount = scanResult.UnknownFilesCount, BadFileType = "Unknown", Name = scanResult.HostName, Uid = scanResult.TargetUid });

                    devices.Add(new VulnerableDeviceInfoEx() { BadFilesCount = scanResult.MalwareFilesCount, BadFileType = "Malicious", Name = scanResult.HostName, Uid = scanResult.TargetUid });

                    devices.Add(new VulnerableDeviceInfoEx() { BadFilesCount = scanResult.NotAnalyzedFilesCount, BadFileType = "Not Analyzed", Name = scanResult.HostName, Uid = scanResult.TargetUid });

                    devices.Add(new VulnerableDeviceInfoEx() { BadFilesCount = scanResult.PendingAnalysisFilesCount, BadFileType = "In Analysis", Name = scanResult.HostName, Uid = scanResult.TargetUid });
                }

                return devices.ToArray();
            }
        }

        public override ItemRatingInfo[] FileRating
        {
            get
            {
                var total = SafeFilesCount + UnknownFilesFound + MalwareFilesFound + PendingAnalysisFilesCount + NotAnalyzedFilesCount;
                if (total == 0) return new ItemRatingInfo[0];
                var itemRatingInfoList = new List<ItemRatingInfo>
                {
                    new FileRatingInfoFAT{ CloudVerdict = (int)FinalVerdict.Clean, Count = SafeFilesCount, Value = string.Format("Trusted: ({0:P2})", total == 0?0: SafeFilesCount / (double) total)},
                    new FileRatingInfoFAT{ CloudVerdict = (int)FinalVerdict.Unknown, Count = UnknownFilesFound, Value = string.Format("Unknown  ({0:P2})", total == 0?0:UnknownFilesFound / (double) total)},
                    new FileRatingInfoFAT{ CloudVerdict = (int)FinalVerdict.Malware, Count = MalwareFilesFound, Value = string.Format("Malicious ({0:P2})", total == 0?0:MalwareFilesFound / (double) total)},
                    new FileRatingInfoFAT{ CloudVerdict = (int)FinalVerdict.InProgress, Count = PendingAnalysisFilesCount, Value = string.Format("In Analysis ({0:P2})" , total == 0?0:PendingAnalysisFilesCount / (double) total )},
                    new FileRatingInfoFAT{ CloudVerdict = (int)FinalVerdict.NotAnalyzed, Count = NotAnalyzedFilesCount, Value = string.Format("Not Analyzed ({0:P2})" , total == 0?0:NotAnalyzedFilesCount / (double) total )}
                };

                return itemRatingInfoList.ToArray();
            }
        }

        public override ItemRatingInfo[] DevicesRating
        {
            get
            {
                IEnumerable<ScannedTarget> targets;
                IEnumerable<DetailedBadFile> files;

                MergeResults(_Targets, ValkyrieAnalysisResult, out targets, out files, out files);

                List<ITargetVerdicted> list = new List<ITargetVerdicted>();

                list.AddRange(targets);
                list.AddRange(_Targets.Where(t => t.TargetVerdict == TargetVerdict.Unknown));
                return CalculateTargetVerdict(list);
            }
        }
    }
}
