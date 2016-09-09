using APTAT.DeploymentCore;
using System;
using System.ComponentModel;

namespace DDT.Core.Reports
{
    public enum DeploymentStatus
    {
        Failed,
        Skipped,
        Stopped,
        Succeded
    }

    public class DetailsOfScan : IEquatable<DetailsOfScan>, IDetailsOfScan
    {
        public string IP { get; set; }
             
        public string DeviceName { get; set; }

        public string OS { get; set; }        
        public Guid TargetUid { get; set; }

        public SystemFamilyKind SystemFamilyKind { get; set; }
        public string Details { get; set; }
        public DeploymentStatus DeploymentStatus { get; set; }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = obj as DetailsOfScan;

            return Equals(other);
        }


        public string PackageName { get; set; }

        public override int GetHashCode()
        {
            return this.TargetUid.GetHashCode();
        }
        public bool Equals(DetailsOfScan other)
        {
            if (other == null) return false;

            return this.TargetUid == other.TargetUid;
        }
    }
}
