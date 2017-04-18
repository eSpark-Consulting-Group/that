namespace APTAT.Core.CMC.Request
{
    public interface IScanStartedRequest : IRequest
    {
        ScanTriggerType ScanTriggerType { get; set; }
        ScanTarget ScanTarget { get; set; }
        ScanType ScanType { get; set; }
        int TotalCountOfDiscoveredComputers { get; set; }
        int CountOfScannedComputers { get; set; }
    }
}