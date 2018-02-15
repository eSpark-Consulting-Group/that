using System;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Directory;
using System.Collections.Generic;

namespace UFH.Application.Models
{
    public interface IScanOperationCollection : IList<IScanOperation>
    {

    }

    public interface IScanOperation
    {
        string DiscovererDescription { get; set; }
        DateTime Started { get; set; }
        DateTime? Finished { get; }
        Guid OperationUid { get; set; }
        ScanKind ScanMethod { get; set; }
        IDevicesSourceDescription DevicesSourceDescription { get; set; }
        int ComputersScanned { get; set; }
        int TotalComputersScanned { get; set; }
        int FilesScanned { get; set; }
        int UnknownsCount { get; set; }
        int NotAnalyzedCount { get; set; }
        int MaliciousCount { get; set; }
        int InAnalysisCount { get; set; }
        int CleanCount { get; set; }
        bool IsFailed { get; set; }
        bool IsCancelled { get; set; }
        IValkyrieOperationInfo ValkyrieOperationInfo { get; set; }
    }

    public interface IValkyrieOperationInfo
    {
        string ApiKey { get; }
        string ScanId { get; }
        string DetailedLink { get; set; }
    }
}