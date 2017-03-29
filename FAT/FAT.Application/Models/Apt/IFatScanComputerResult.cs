using System;
using System.Collections.Generic;

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
        string Name { get; set; }
        string IpAddress { get; set; }
        bool IsLocal { get; set; }
        bool IsOnline { get; set; }
    }

    public class ScanComputerResultComparer : IComparer<IFatScanComputerResult>
    {
        public int Compare(IFatScanComputerResult x, IFatScanComputerResult y)
        {
            if ((x.IsLocal && !y.IsLocal) || (x.IsOnline && !y.IsOnline))
                return -1;

            if ((!x.IsLocal && y.IsLocal) || (!x.IsOnline && y.IsOnline))
                return 1;

            if (char.IsDigit(x.Name[0]) && !char.IsDigit(y.Name[0]))
                return -1;

            if (!char.IsDigit(x.Name[0]) && char.IsDigit(y.Name[0]))
                return 1;

            return x.Name.CompareTo(y.Name);
        }
    }

    public interface IScanOperationResult
    {
        IScanOperation ScanOperation { get; }
        IFatScanComputerResult[] ComputerResults { get; }
    }
}
