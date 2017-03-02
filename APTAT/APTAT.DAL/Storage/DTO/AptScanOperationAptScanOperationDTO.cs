using APTAT.DAL;
using System;

namespace APTAT.Core.Storage.Repositories
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

        //public 
    }
}