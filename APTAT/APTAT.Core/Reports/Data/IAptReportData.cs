using System;
using System.Collections.Generic;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Directory;

namespace APTAT.Core.Reports
{
    public interface IAptReportDataValkyrie
    {
        #region Valkyrie
        Dictionary<string, FileResult> ValkyrieAnalysisResult { get; set; }
        ItemRatingInfo[] FileRatingValkyrie { get; }
        ItemRatingInfo[] DevicesRatingValkyrie { get; }
        VulnerableDeviceInfoEx[] Top10VulnerableDevicesValkyrie { get; }
        ScanResultValkyrie[] SucceededTargetsWithValkyrie { get; }
        Guid ValkyrieOperationUid { get; set; }

        #endregion
    }

    public interface IAptReportData
    {        
        IDevicesSourceDescription DevicesSourceDescription { get; set; }
        string DevicesSourceDescriptionString { get; }
        int EmptyFilesFound { get; }
        int FailedToAnalyzeFilesCount { get; }
        
        bool IsActiveDirectoryDevicesSource { get; }
        int LookupInProgressFilesCount { get; }
        int MalwareFilesFound { get; }
        int NetworkTrustLevel { get; }
        string NetworkTrustLevelString { get; }
        int NotAnalyzedFilesCount { get; }
        int NoVerdictFilesCount { get; }
        string OperationUid { get; set; }
        int PendingAnalysisFilesCount { get; }
        int SafeFilesCount { get; }
        DateTime ScanFinished { get; set; }
        DateTime ScanStarted { get; set; }
        int SkippedFilesCount { get; }
        ScanResult[] SucceededTargets { get; }
        
        int TargetsFailed { get; set; }
        int TargetsNotPerformed { get; set; }
        AptRiskAssessmentLevel TargetsSafetyLevel { get; }
        string TargetsSafetyLevelString { get; }
        int TargetsScanned { get; }
        int TargetsSucceeded { get; set; }        
        int TotalErrorFilesCount { get; }
        int TotalFilesScanned { get; }
        int TotalTargetsCount { get; set; }
        int UnknownFilesFound { get; }
        
        
        
        MostWantedBadFileInfo[] GetTop10PopularBadFiles(DetailedBadFile[] files);
        MostWantedBadFileInfo[] Top10PopularBadFiles { get; }
        VulnerableDeviceInfoEx[] Top10VulnerableDevices { get; }        
        ItemRatingInfo[] DevicesRating { get; }        
        ItemRatingInfo[] FileRating { get; }
        

        //void SetTargets(IAptScanResult[] targets);

 
    }
}