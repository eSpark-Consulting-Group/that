using System;

namespace APTAT.Application.Models
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
        OperationCompleted,
        OperationFailed,
        OperationCanceled
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

        public Int32 Percentage { get; set; }

        public Int32 FailedToAnalyzeFilesCount { get; set; }

        public Int32 NoVerdictFilesCount { get; set; }

    }
}
