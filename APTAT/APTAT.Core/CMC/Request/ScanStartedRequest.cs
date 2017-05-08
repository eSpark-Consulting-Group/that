using APTAT.DeploymentCore.Directory;

namespace APTAT.Core.CMC.Request
{
    public class ScanStartedRequest: IScanStartedRequest
    {
        public ScanTriggerType ScanTriggerType { get; set; }
        public ETargetsDiscovererTypeRequest ScanTarget { get; set; }
        public ScanType ScanType { get; set; }
        public int TotalCountOfDiscoveredComputers { get; set; }
        public int CountOfScannedComputers { get; set; }
    }
}
