using APTAT.DeploymentCore;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace DDT.Core.Reports
{
    public interface IDetailsOfScan
    {
        [DataMember(Name = "IP", Order = 0)]
        string IP { get; set; }
        
        [DataMember(Name = "Device Name", Order = 1)]
        string DeviceName { get; set; }

        [DataMember(Name = "OS", Order = 2)]
        string OS { get; set; }


        [IgnoreDataMember]
        Guid TargetUid { get; set; }

        [IgnoreDataMember]
        SystemFamilyKind SystemFamilyKind { get; set; }

        [DataMember(Name = "Details", Order = 4)]
        string Details { get; set; }

        [DataMember(Name = "Status", Order = 3)]
        DeploymentStatus DeploymentStatus { get; set; }


        [DataMember(Name = "Package", Order = 5)]
        string PackageName { get; set; }
    }
}
