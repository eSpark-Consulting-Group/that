namespace APTAT.Core.CMC.Request
{
    public interface IScanUpdatedRequest : IRequest
    {
        int CountOfNewUnknownFilesWhichWasFoundAfterTheScanWasFinished { get; set; }
        int CountOfNewMaliciousFilesWhichWasFoundAfterTheScanWasFinished { get; set; }
    }
}