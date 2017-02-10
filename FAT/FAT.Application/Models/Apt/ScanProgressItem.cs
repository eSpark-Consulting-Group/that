using System;

namespace FAT.Application.Models
{
    public enum ScanStage
    {
        Unknown,
        Pending,
        Connecting,
        CopyingScanner,
        ConfiguringScanner,
        StartingScanner,
        ScanInProgress,
        ScanCompleted,
        ScanFailed,
        ScanResultsObtaning,
        ScanResultsObtaned,
        ScanResultsProcessing,
        DescentOperationCompleted,
        OperationFailed,
        OperationCanceled,
        OperationCompleted,
        RemoteOperationCompleted,
        OperationCancelling
    }

    public class ScanProgressItem : DomainObject
    {
        public Guid ComputerGuid { get; set; }

        public ScanStage ScanStage { get; set; }
        
        public String StatusMessage { get; set; }

        public Int32 TotalFilesCount { get; set; }

        public Int32 UnknownFilesCount { get; set; }

        public Int32 MalwareFilesCount { get; set; }

        public Guid OperationUid { get; set; }

        public Int32 FailedToAnalyzeFilesCount { get; set; }

        public Int32 LookupInProgress => FailedToAnalyzeFilesCount+NoVerdictFilesCount;

        public Int32 NoVerdictFilesCount { get; set; }
        public double ProgressPercentage { get; set; }
    }
}
