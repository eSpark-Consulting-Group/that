namespace APTAT.Core.CMC.Request
{
    public class ScanUpdatedRequest: IScanUpdatedRequest
    {
        public int CountOfNewUnknownFilesAfterTheScanWasFinished { get; set; }
        public int CountOfNewMaliciousFilesAfterTheScanWasFinished { get; set; }
    }
}
