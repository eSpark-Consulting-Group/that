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

        public String StatusMessage
        {
            get
            {
                switch (ScanStage)
                {
                    case ScanStage.Unknown:
                    case ScanStage.Pending:
                    case ScanStage.Connecting:
                    case ScanStage.CopyingScanner:
                    case ScanStage.ConfiguringScanner:
                    case ScanStage.StartingScanner:
                    case ScanStage.ScanFailed:
                    case ScanStage.OperationFailed:
                    case ScanStage.OperationCanceled:
                        return StatusMessageSystem;
                        
                        
                    case ScanStage.ScanCompleted:
                    case ScanStage.ScanInProgress:
                    case ScanStage.ScanResultsObtaning:
                    case ScanStage.ScanResultsObtaned:
                    case ScanStage.ScanResultsProcessing:
                    case ScanStage.OperationCompleted:
                    
                        return GetStatusMessage();
                    
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            }
        
        }

        private string GetStatusMessage()
        {
            
            string errorCounterText = LookupInProgress > 0 ?
                    $"Lookup in progress {LookupInProgress}."
                    : String.Empty;

            string serverErrorCounterText = NoVerdictFilesCount > 0 ? $"No Verdict: {NoVerdictFilesCount}."
                : String.Empty;

            var text = string.Format("{0} files scanned ({3}%). Unknown files: {1}. Malicious files: {2}. {4} {5}", TotalFilesCount, UnknownFilesCount, MalwareFilesCount, string.Format("{0:0.00}", ProgressPercentage).Replace(".00", string.Empty), errorCounterText, serverErrorCounterText);

            return text;
        }

        public String StatusMessageSystem { get; set; }

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
