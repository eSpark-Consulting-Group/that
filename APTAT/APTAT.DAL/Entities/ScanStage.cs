namespace APTAT.DAL.Entities
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
}