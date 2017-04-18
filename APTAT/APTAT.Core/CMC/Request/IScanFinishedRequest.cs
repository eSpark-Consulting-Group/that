namespace APTAT.Core.CMC.Request
{
    public interface IScanFinishedRequest : IRequest
    {
        ScanStatus ScanStatus { get; set; }
        int CountOfSuccessScans { get; set; }
        int CountOfFailedScans { get; set; }
        int CountOfUnknownFiles { get; set; }
        int CountOfMaliciousFiles { get; set; }
    }
}