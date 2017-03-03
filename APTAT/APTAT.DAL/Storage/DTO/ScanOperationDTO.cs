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
        public DateTime ScanDate { get; set; }
        public ScanKind ScanMethod { get; set; }

        public ETargetsDiscovererType Type { get; set; }
        public string DiscovererDescription { get; set; }

    }

    public enum ETargetsDiscovererType
    {
        ActiveDirectory,
        NetworkAddresses,
        Workgroup
    }
}

