using System;

namespace APTAT.Application.Models
{
    public class ScanResultItem
    {
        public Guid TargetUid { get; set; }

        public Int32 TotalFilesCount { get; set; }

        public Int32 UnknownFilesCount { get; set; }

        public BadFileItem[] BadFiles { get; set; }

        public ScanStage ScanStage { get; set; }
        public string StatusMessage { get; set; }
    }
}
