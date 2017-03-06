using System;
using APTAT.DAL.Enums;

namespace APTAT.DAL.Storage.DTO
{
    public interface IAptScanOperationDTO
    {
        string DiscovererDescription { get; set; }
        int FilesCount { get; set; }
        DateTime? Finished { get; }
        int MaliciousCount { get; set; }
        Guid OperationUid { get; set; }
        ScanKind ScanMethod { get; set; }
        string SourceDescription { get; set; }
        DateTime Started { get; set; }
        int TotalDevicesScanned { get; set; }
        ETargetsDiscovererType Type { get; set; }
        int UnknownsCount { get; set; }
        IValkyrieOperationInfo ValkyrieOperationInfo { get; set; }
    }
}