using System.Collections.Generic;
using APTAT.DAL.Entities;
using APTAT.DAL.Storage.DTO;

namespace APTAT.Core.Storage.Repositories
{
    public class AptScanResultDTO
    {
        public AptScanResult ScanResult { get; internal set; }
        public IEnumerable<ModuleWithValkyrie> Modules { get; internal set; }  
        public int TotalCount { get; internal set; }
        public int UnknownsCount { get; internal set; }
        public int MaliciousCount { get; internal set; }
        public int NotAnalyzedCount { get; internal set;}
        public int InAnalysisCount { get; internal set; }
    }
}