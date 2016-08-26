using APTAT.DeploymentCore;
using System;
using System.ComponentModel;

namespace DDT.Core.Reports
{
    public interface IDetailsOfScan
    {
        [Description("IP")]
        string IP { get; set; }
        [Description("Device Name")]
        string DeviceName { get; set; }
        string OS { get; set; }

        [Description("Target Uid")]
        Guid TargetUid { get; set; }

        [Description("System Family")]
        SystemFamilyKind SystemFamilyKind { get; set; }
        string Details { get; set; }
        [Description("Status")]
        DeploymentStatus DeploymentStatus { get; set; }
        [Description("Package Name")]
        string PackageName { get; set; }
    }
}
