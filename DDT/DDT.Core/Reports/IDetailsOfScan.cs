using APTAT.DeploymentCore;
using System;

namespace DDT.Core.Reports
{
    public interface IDetailsOfScan
    {
        string IP { get; set; }
        string DeviceName { get; set; }
        string OS { get; set; }
        string Status { get; set; }
        Guid TargetUid { get; set; }
        SystemFamilyKind SystemFamilyKind { get; set; }
        string Details { get; set; }
        DeploymentStatus DeploymentStatus {get;set;}
        string PackageName { get; set; }
    }
}
