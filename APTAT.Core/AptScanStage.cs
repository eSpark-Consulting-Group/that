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
        ScanFailed
    }
}
