using System;

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
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public bool IsLocal { get; set; }
        public bool IsOnline { get; set; }
    }

    public class ScanOperationResult : IScanOperationResult
    {
        private IFatScanComputerResult[] _computerResults;
        private IScanOperation _scanOperation;

        public ScanOperationResult(
            IScanOperation scanOperation, 
            IFatScanComputerResult[] computerResults)
        {
            _scanOperation = scanOperation;
            _computerResults = computerResults;
        }

        public IFatScanComputerResult[] ComputerResults => _computerResults;

        public IScanOperation ScanOperation => _scanOperation;
    }
}
