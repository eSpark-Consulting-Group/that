using System;

namespace FAT.Application.Models
{
    public interface IScanResultItem
    {
        IBadFileItem[] BadFiles { get; set; }
        int CleanFilesCount { get; set; }
        int FailedToAnalyzeFilesCount { get; set; }
        int MalwareFilesCount { get; set; }
        TimeSpan Modified { get; set; }
        int NoVerdictFilesCount { get; set; }
        double Percentage { get; set; }
        ScanStage ScanStage { get; set; }
        string StatusMessage { get; set; }
        Guid TargetUid { get; set; }
        int TotalFilesCount { get; set; }
        int UnknownFilesCount { get; set; }
    }
}