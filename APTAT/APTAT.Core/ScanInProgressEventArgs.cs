using System;

namespace APTAT.Core
{
    public class ScanInProgressEventArgs : EventArgs
    {
        public Guid OperationUid { get; set; }
        public double Percentage { get; set; }
    }
}