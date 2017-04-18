namespace APTAT.Core.CMC.Request
{
    public class ScanFinishedRequest: IScanFinishedRequest
    {
        public ScanStatus ScanStatus { get; set; }
        public int CountOfSuccessScans { get; set; }
        public int CountOfFailedScans { get; set; }
        public int CountOfUnknownFiles { get; set; }
        public int CountOfMaliciousFiles { get; set; }
    }
}
