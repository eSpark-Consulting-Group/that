using APTAT.Core.Reports;
using APTAT.DeploymentCore.Descent;

namespace APTAT.Core.Descent.Apt
{
    public interface IAptScanResultInternal : IAptScanResult
    {
        void SetCompleted();
    }
    public interface IAptScanResult : IComputerDescentResult, ITargetVerdicted
    {        
        string FileName { get; set; }
        int TotalFilesCount { get; set; }
        int UnknownFilesCount { get; set; }
        int SafeFilesCount { get; set; }
        int MalwareFilesCount { get; set; }
        int EmptyFilesCount { get; set; }
        int LookupInProgressFilesCount { get; }
        int FailedToAnalyzeFilesCount { get; set; }
        int NoVerdictFilesCount { get; set; }
        int PendingAnalysisFilesCount { get; set; }
        int TotalErrorFilesCount { get; }
        int SkippedFilesCount { get; set; }
        int RunningProgramsCount { get; set; }
        int AutorunsCount { get; set; }
        string ResultErrorText { get; set; }
        AptScanStage ScanStage { get; set; }
        ScanResultType ResultType { get; set; }
        string TargetVerdictString { get; }
        IModule[] Files { get; }
        IModule[] BadFiles { get; }
        AptReport AptReport { get; }
        IModule[] GetFilesByStatus(CloudVerdict cloudVerdict);        
        IModule[] GetBadFiles();         
        void RecalculateStatistics();
        IModule[] MalwareFiles { get; }
        IModule[] UnknownFiles { get; }
        IModule[] LookupInProgressFiles { get; }
        void SetAptReportInstance(AptReport report);


        int BadFilesCount { get;  }
    }
}