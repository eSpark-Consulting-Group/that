namespace APTAT.Core.CMC.Request
{
    public class AutoDiscoveryFinishedRequest: IAutoDiscoveryFinishedRequest
    {
        public DiscoveryStatus DiscoveryStatus { get; set; } 
        public int CountOfPotentiallyScanableDiscoveredComputers { get; set; }
    }
}
