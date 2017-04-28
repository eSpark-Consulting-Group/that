namespace APTAT.Core.CMC.Request
{
    public interface IScanUpdatedRequest : IRequest
    {
        int CountOfNewUnknownFilesAfterTheScanWasFinished { get; set; }
        int CountOfNewMaliciousFilesAfterTheScanWasFinished { get; set; }
    }
}