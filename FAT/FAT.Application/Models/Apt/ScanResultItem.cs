using System;

namespace FAT.Application.Models
{
    public class ScanResultItem : IScanResultItem
    {
        public Guid TargetUid { get; set; }

        public Int32 TotalFilesCount { get; set; }

        public Int32 UnknownFilesCount { get; set; }

        public IBadFileItem[] BadFiles { get; set; }

        public ScanStage ScanStage { get; set; }
        public TimeSpan Modified { get; set; }
        public string StatusMessage { get; set; }
        public int FailedToAnalyzeFilesCount { get; set; }
        public int NoVerdictFilesCount { get; set; }
        public int MalwareFilesCount { get; set; }
        public double Percentage { get; set; }
        public int CleanFilesCount { get; set; }
    }
}
