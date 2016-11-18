using System;
using APTAT.Core.Reports;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;
using APTAT.DeploymentCore.Messages;

namespace APTAT.Core.Descent.Apt
{
    public interface IAptScanResult : IComputerDescentResult
    {
        string OsName { get; }
        string FileName { get; set; }
        int TotalFilesCount { get; set; }
        int UnknownFilesCount { get; set; }
        int SafeFilesCount { get; set; }
        int MalwareFilesCount { get; set; }
        int EmptyFilesCount { get; set; }
        int NotAnalyzedFilesCount { get; }
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
        int BadFilesCount { get; }
        TargetVerdict TargetVerdict { get; }
        string TargetVerdictString { get; }
        Module[] Files { get; }
        Module[] BadFiles { get; }
        AptReport AptReport { get; }
        Module[] GetFilesByStatus(CloudVerdict cloudVerdict);
        Module[] GetUnknownFiles();
        Module[] GetFailedToAnalyzeFiles();
        Module[] GetNoVerdictFiles();
        Module[] GetBadFiles();
        Module[] GetUknownFiles();
    }
}