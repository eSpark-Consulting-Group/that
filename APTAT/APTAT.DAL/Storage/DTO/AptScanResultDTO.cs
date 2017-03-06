using System.Collections.Generic;
using APTAT.DAL.Entities;

namespace APTAT.Core.Storage.Repositories
{
    public class AptScanResultDTO
    {        
        public AptScanResult ScanResult { get; internal set; }
        public int TotalCount { get; internal set; }
        public int UnknownsCount { get; internal set; }
        public int MaliciousCount { get; internal set; }
        public int SafeFilesCount { get; internal set; }
        public int NotAnalyzedFilesCount { get; internal set;}
    }
}