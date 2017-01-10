using System;

namespace APTAT.Core
{
    public class ScanCompletedEventArgs : EventArgs
    {
        public Guid OperationUid { get; set; }
        public int TotalCount { get; set; }
        public int NotPerformedCount { get; set; }
        public int SucceededCount { get; set; }
        public int FailedCount { get; set; }
        public int CanceledCount { get; set; }
        public IFilesStatistics FilesStatistics { get; set; }
    }
}