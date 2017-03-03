using System;
using APTAT.DAL.Enums;

namespace APTAT.DAL.Storage.DTO
{
    public class AptScanOperationDTO : IAptScanOperationDTO
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