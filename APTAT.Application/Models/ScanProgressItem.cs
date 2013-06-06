using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Application.Models
{
    public enum ScanStage
    {
        Pending,
        Connecting,
        CopyingScanner,
        ConfiguringScanner,
        StartingScanner,
        ScanInProgress,
        ScanCompleted,
        ScanFailed,
        ScanResultsProcessing,
        OperationCompleted,
        OperationFailed
    }

    public class ScanProgressItem : DomainObject
    {
        public Guid ComputerGuid { get; set; }

        public ScanStage ScanStage { get; set; }

        public String StatusMessage { get; set; }

        public Int32 TotalFilesCount { get; set; }

        public Int32 UnknownFilesCount { get; set; }

        public Guid OperationUid { get; set; }

        public Int32 Percentage { get; set; }
    }
}
