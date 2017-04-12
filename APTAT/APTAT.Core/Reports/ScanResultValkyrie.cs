using APTAT.Core.Descent.Apt;
using System;

namespace APTAT.Core.Reports
{

    public class ScanResultValkyrie : ScanResult
    {
        public DetailedBadFile[] PendingAnalysisFilesValkyrie { get; internal set; }
        public DetailedBadFile[] MaliciousFilesValkyrie { get; internal set; }
        public DetailedBadFile[] FailedToAnalyzeFilesValkyrie { get; internal set; }
        public DetailedBadFile[] UknownFilesValkyrie { get; internal set; }

        public int SeverityIndex
        {
            get
            {
                switch (TargetVerdict)
                {
                    case TargetVerdict.Infected:
                        return 0;
                    case TargetVerdict.Vulnerable:
                        return 1;
                    case TargetVerdict.Unknown:
                        return 2;
                    case TargetVerdict.Safe:
                        return 3;
                    case TargetVerdict.NotScanned:
                        return 4;
                    default:
                        return 5;
                }

            }
        }

        public DetailedBadFile[] EndPointFiles { get; set; }
    }
}