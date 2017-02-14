using APTAT.Core.Reports;
using APTAT.DeploymentCore.Descent;

namespace APTAT.Core.Descent.Apt
{
    public interface IAptScanResultInternal : IAptScanResult
    {
        void SetCompleted();
    }
    public interface IAptScanResult : IComputerDescentResult
    {
        string OsName { get; }
        string HostName { get; }
        string OrganizationUnit { get; }
        string IPAddress { get; }
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
        TargetVerdict TargetVerdict { get; }
        string TargetVerdictString { get; }
        IModule[] Files { get; }
        IModule[] BadFiles { get; }
        AptReport AptReport { get; }
        IModule[] GetFilesByStatus(CloudVerdict cloudVerdict);
        //IModule[] GetUnknownFiles();
        //IModule[] GetFailedToAnalyzeFiles();
        //IModule[] GetNoVerdictFiles();
        IModule[] GetBadFiles();      
   //     IModule[] GetLookupInProgressFiles();
        void RecalculateStatistics();
        ScanModule[] MalwareFiles { get; }               
        ScanModule[] UnknownFiles { get; }
        ScanModule[] LookupInProgressFiles { get; }
        DetailedBadFile[] UknownFilesValkyrie { get;  }
        DetailedBadFile[] MaliciousFilesValkyrie { get; }
        DetailedBadFile[] FailedToAnalyzeFilesValkyrie { get; }
        DetailedBadFile[] PendingAnalysisFilesValkyrie { get; }

    }
}