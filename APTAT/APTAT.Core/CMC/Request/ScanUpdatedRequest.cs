namespace APTAT.Core.CMC.Request
{
    public class ScanUpdatedRequest: IScanUpdatedRequest
    {
        public int CountOfNewUnknownFilesWhichWasFoundAfterTheScanWasFinished { get; set; }
        public int CountOfNewMaliciousFilesWhichWasFoundAfterTheScanWasFinished { get; set; }
    }
}
