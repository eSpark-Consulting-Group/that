using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAT.Application.Models.Apt
{
    public interface IFatScanComputerResult
    {
        IFatFileResult[] BadFiles { get; set; }
        int CleanFilesCount { get; set; }
        int MalwareFilesCount { get; set; }
        int InAnalysisFilesCount { get; set; }
        ScanStage ScanStage { get; set; }
        string StatusMessage { get; set; }
        Guid TargetUid { get; set; }
        int TotalFilesCount { get; set; }
        int UnknownFilesCount { get; set; }
        int NotAnalyzedFilesCount { get; set; }
    }
}
