using System;
using System.Linq;
using APTAT.Core.Descent.Apt;
using OperatingSystem = APTAT.Core.Descent.Apt.OperatingSystem;

namespace APTAT.Core
{
    public class ComputerInformation : IEquatable<ComputerInformation>
    {
        public ComputerInformation(Computer computer, Guid targetUid)
        {
            Os = computer.Os;
            Name = computer.Name;
            Sid = computer.Sid;
            TargetUid = targetUid;
            if (computer.Connections.Any())
            {
                IP = computer.Connections.First().IPv4;
            }
        }

        public string Name { get; set; }
        public string Sid { get; set; }
        public OperatingSystem Os { get; set; }
        public string IP { get; set; }
        public string FilePath { get; set; }
        public Guid TargetUid { get; set; }

        public bool Equals(ComputerInformation other)
        {
            return TargetUid == other.TargetUid && FilePath == other.FilePath;
        }
    }
}
