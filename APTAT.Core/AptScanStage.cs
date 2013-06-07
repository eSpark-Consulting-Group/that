namespace APTAT.Core
{
    public enum AptScanStage
    {
        Pending,
        Connecting,
        CopyingScanner,
        ConfiguringScanner,
        StartingScanner,
        ScanInProgress,
        ScanCompleted,
        ScanFailed,
        ScanResultsObtaning,
        ScanResultsProcessing,
        OperationCompleted,
        OperationFailed,
        OperationCanceled
    }
}
