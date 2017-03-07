using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAT.Application.Models.Apt
{
    public class FatScanComputerResult : IFatScanComputerResult
    {
        public IFatFileResult[] BadFiles { get; set; }
        public int CleanFilesCount { get; set; }
        public int MalwareFilesCount { get; set; }
        public int InAnalysisFilesCount { get; set; }
        public ScanStage ScanStage { get; set; }
        public string StatusMessage { get; set; }
        public Guid TargetUid { get; set; }
        public int TotalFilesCount { get; set; }
        public int UnknownFilesCount { get; set; }
        public int NotAnalyzedFilesCount { get; set; }
    }
}
