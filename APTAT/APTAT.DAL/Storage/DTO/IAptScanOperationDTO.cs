using System;
using APTAT.DAL.Enums;

namespace APTAT.DAL.Storage.DTO
{
    public interface IAptScanOperationDTO
    {
        string DiscovererDescription { get; set; }
        int FilesScanned { get; set; }
        DateTime? Finished { get; }
        Guid OperationUid { get; set; }
        ScanKind ScanMethod { get; set; }
        string SourceDescription { get; set; }
        DateTime Started { get; set; }
        int ComputersScanned { get; set; }
        int TotalComputersScanned { get; set; }
        ETargetsDiscovererType? Type { get; set; }
        int MaliciousCount { get; set; }
        int UnknownsCount { get; set; }
        int InAnalysisCount { get; set; }
        int NotAnalyzedCount { get; set; }
        bool Canceled { get; }
        IValkyrieOperationInfo ValkyrieOperationInfo { get; set; }
    }
}