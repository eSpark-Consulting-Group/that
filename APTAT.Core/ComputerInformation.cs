using System;
using System.Collections.Generic;
using System.Linq;

namespace APTAT.Core
{
    public class ComputerInformation
    {
        public ComputerInformation(Computer computer)
        {
            Os = computer.Os;
            Name = computer.Name;
            Sid = computer.Sid;
            if (computer.Connections.Any())
            {
                this.IP = computer.Connections.First().IPv4;

            }
        }

        public string Name { get; set; }
        public string Sid { get; set; }
        public OperatingSystem Os { get; set; }


        public string IP { get; set; }
        public string FilePath { get; set; }
        public string FullName
        {
            get
            {

                return String.Format("{0} ({1}) {2}", this.Name, this.IP, this.Os);

            }
        }
    }
}
