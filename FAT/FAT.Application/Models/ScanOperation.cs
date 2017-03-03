using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Directory;

namespace FAT.Application.Models
{
    public class ScanOperation : IScanOperation
    {
        public int TotalDevicesScanned { get; set; }
        public int FilesCount { get; set; }
        public int UnknownsCount { get; set; }
        public int MaliciousCount { get; set; }
        public System.Guid OperationUid { get; set; }
        public ScanKind ScanMethod { get; set; }
        public string DiscovererDescription { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Finished { get; internal set; }
        public IDevicesSourceDescription DevicesSourceDescription { get; set; }
    }
}
