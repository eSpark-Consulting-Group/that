using System;

namespace FAT.Application.Models.Apt
{
    public interface IFatScanComputerResult
    {
        IFatFileResult[] BadFiles { get; set; }
        int CleanFilesCount { get; set; }
        int MalwareFilesCount { get; set; }
        int InAnalysisFilesCount { get; set; }
        ScanStage ScanStage { get; set; }
        string StatusMessage { get; set; }
        Guid TargetUid { get; set; }
        int TotalFilesCount { get; set; }
        int UnknownFilesCount { get; set; }
        int NotAnalyzedFilesCount { get; set; }
        string Name { get; set; }
        string IpAddress { get; set; }
        bool IsLocal { get; set; }
        bool IsOnline { get; set; }
    }

    public interface IScanOperationResult
    {
        IScanOperation ScanOperation { get; }
        IFatScanComputerResult[] ComputerResults { get; }
    }
}
