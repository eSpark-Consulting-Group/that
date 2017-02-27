using System;
using System.Collections.Generic;
using System.Text;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore;

namespace APTAT.Core.Reports
{
    public class ScannedTarget
    {
        private readonly List<DetailedBadFile.BadFileInstanceInfo> _badFileInstances = new List<DetailedBadFile.BadFileInstanceInfo>();

        public ScannedTarget()
        {
        }

        public ScannedTarget(IAptScanResult aptScanResult)
        {
            if (aptScanResult == null) throw new ArgumentNullException(nameof(aptScanResult));
            Computer = aptScanResult.Computer;
            TargetUid = aptScanResult.TargetUid;
            HostName = aptScanResult.Computer?.Name;
            IpAddress = aptScanResult.Computer?.IPAddress;
            OsName = aptScanResult.AptReport.Computer?.Os?.ToString();
            OrganizationUnit = aptScanResult.Computer.OrganizationUnit;
            TotalFilesCount = aptScanResult.TotalFilesCount;
            UnknownFilesCount = aptScanResult.UnknownFilesCount;
            FailedToAnalyzeFilesCount = aptScanResult.FailedToAnalyzeFilesCount;
            NoVerdictFilesCount = aptScanResult.NoVerdictFilesCount;
            PendingAnalysisFilesCount = aptScanResult.PendingAnalysisFilesCount;
            SafeFilesCount = aptScanResult.SafeFilesCount;
            MalwareFilesCount = aptScanResult.MalwareFilesCount;
            RunningProgramsCount = aptScanResult.RunningProgramsCount;
            AutorunsCount = aptScanResult.AutorunsCount;
            TargetVerdictString = aptScanResult.TargetVerdictString;
            AptReport = aptScanResult.AptReport;
            ResultErrorText = aptScanResult.ResultErrorText;
            ResultType = aptScanResult.ResultType;
        }
        public IComputer Computer { get; }

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

        public int NotAnalyzedFilesCount => FailedToAnalyzeFilesCount + NoVerdictFilesCount;
        public int FailedToAnalyzeFilesCount { get; set; }
        public int NoVerdictFilesCount { get; set; }
        public int RunningProgramsCount { get; set; }
        public int PendingAnalysisFilesCount { get; set; }
        public string TargetVerdictString { get; set; }

        public TargetVerdict TargetVerdict
        {
            get
            {
                if (MalwareFilesCount > 0)
                    return TargetVerdict.Infected;
                if (UnknownFilesCount > 0 || FailedToAnalyzeFilesCount > 0 || NoVerdictFilesCount > 0 || PendingAnalysisFilesCount > 0)
                    return TargetVerdict.Vulnerable;
                return TargetVerdict.Safe;

            }
        }
        public int AutorunsCount { get; set; }

        public int InstancesCount
        {
            get { return BadFileInstances.Count; }
        }

        public int BadFilesCount
        {
            get { return UnknownFilesCount + MalwareFilesCount + TotalFilesCount + FailedToAnalyzeFilesCount; }
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
}