namespace APTAT.Core
{
    public enum AptScanStage
    {
        Pending,
        Connecting,
        CopyingScanner,
        ConfiguringScanner,
        StartingScanner,
        EnumeratingProcesses,
        EnumeratingDrivers,
        EnumeratingAutoruns,
        EnumeratingFolders,
        ScanInProgress,
        ScanCompleted,
        ScanFailed,
        ScanResultsObtaning,
        ScanResultsProcessing,
        OperationCompleted,
        OperationFailed,
        OperationCanceled
    }

    public enum ScanResultType
    {
        NotPerformed,
        Completed,
        Failed,
        Canceled
    }

    public enum EnAptStage
    {
        EnApt_Pending = 0,
        EnApt_EnumeratingProcesses = 1,
        EnApt_EnumeratingDrivers = 2,
        EnApt_EnumeratingAutoruns = 3,
        EnApt_EnumeratingFolder = 4,
        EnApt_ScaningModules = 5,
        EnApt_WritingReport = 6,
        EnApt_Complete = 7,
        EnApt_Failed = 8
    };

}
