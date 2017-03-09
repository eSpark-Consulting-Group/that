using System;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Directory;

namespace FAT.Application.Models
{
    public interface IScanOperation
    {
        string DiscovererDescription { get; set; }
        int FilesCount { get; set; }
        DateTime? Finished { get; }
        int MaliciousCount { get; set; }
        Guid OperationUid { get; set; }
        ScanKind ScanMethod { get; set; }
        DateTime Started { get; set; }
        int TotalDevicesScanned { get; set; }
        int UnknownsCount { get; set; }
        IDevicesSourceDescription DevicesSourceDescription { get; set; }
        IValkyrieOperationInfo ValkyrieOperationInfo { get; set; }
    }

    public interface IValkyrieOperationInfo
    {
        string ApiKey { get; }
        string ScanId { get; }
    }
}