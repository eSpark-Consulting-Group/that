using APTAT.Core.Descent.Apt;
using System;

namespace APTAT.Core.Reports
{
 

    public class ScanResultValkyrie
    {
        public int AutorunsCount { get; internal set; }
        public int FailedToAnalyzeFilesCount { get; internal set; }
        public int MalwareFilesCount { get; internal set; }
        public int NoVerdictFilesCount { get; internal set; }
        public int PendingAnalysisFilesCount { get; internal set; }        
        public string ResultErrorText { get; internal set; }
        public ScanResultType ResultType { get; internal set; }
        public int RunningProgramsCount { get; internal set; }
        public int SafeFilesCount { get; internal set; }
        public int TotalFilesCount { get; internal set; }        
        public int UnknownFilesCount { get; internal set; }
        public string OrganizationUnit { get; internal set; }
        public string HostName { get; internal set; }
        public string IPAddress { get; internal set; }
        public string OsName { get; internal set; }
        public int LookupInProgressFilesCount => FailedToAnalyzeFilesCount + NoVerdictFilesCount;
        
        public DetailedBadFile[] PendingAnalysisFilesValkyrie { get; internal set; }
        public DetailedBadFile[] MaliciousFilesValkyrie { get; internal set; }
        public DetailedBadFile[] FailedToAnalyzeFilesValkyrie { get; internal set; }      
        public DetailedBadFile[] UknownFilesValkyrie { get; internal set; }
        public TargetVerdict TargetVerdict
        {
            get
            {
                if (ResultType == ScanResultType.Failed || ResultType == ScanResultType.Canceled)
                    return TargetVerdict.Unknown;
                if (MalwareFilesCount > 0)
                    return TargetVerdict.Infected;
                if (UnknownFilesCount > 0 || FailedToAnalyzeFilesCount > 0 || NoVerdictFilesCount > 0 || PendingAnalysisFilesCount > 0)
                    return TargetVerdict.Vulnerable;
                if (ResultType == ScanResultType.NotPerformed)
                    return TargetVerdict.NotScanned;
                return TargetVerdict.Safe;
            }
        }

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
        public string TargetVerdictString
        {
            get
            {
                switch (TargetVerdict)
                {
                    case TargetVerdict.Unknown:
                        return "Unknown";
                    case TargetVerdict.Safe:
                        return "Safe";
                    case TargetVerdict.Infected:
                        return "Infected";
                    case TargetVerdict.Vulnerable:
                        return "At Risk";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}