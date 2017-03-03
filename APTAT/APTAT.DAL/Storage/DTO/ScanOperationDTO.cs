using APTAT.DAL.Enums;
using APTAT.DeploymentCore.Directory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.DAL.Storage.DTO
{

    public class AptScanOperationDTO
    {
        public int TotalDevicesScanned { get; set; }
        public int FilesCount { get; set; }
        public int UnknownsCount { get; set; }
        public int MaliciousCount { get; set; }
        public System.Guid OperationUid { get; set; }
        public ScanKind ScanMethod { get; set; }
        public ETargetsDiscovererType Type { get; set; }
        public string DiscovererDescription { get; set; }
        public string SourceDescription { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Finished { get; internal set; }
    }  
}

