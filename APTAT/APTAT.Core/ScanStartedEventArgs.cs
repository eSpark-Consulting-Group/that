using System;

namespace APTAT.Core
{
    public class ScanStartedEventArgs : EventArgs
    {
        public Exception Error { get; set; }
        public Guid OperationUid { get; set; }
        public bool RescanAttempt { get; set; }
        public object State { get; set; }
    }
}