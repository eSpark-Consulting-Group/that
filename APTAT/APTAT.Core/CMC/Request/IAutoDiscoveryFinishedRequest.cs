namespace APTAT.Core.CMC.Request
{
    public interface IAutoDiscoveryFinishedRequest : IRequest
    {
         DiscoveryStatus DiscoveryStatus { get; set; }
         int CountOfPotentiallyScanableDiscoveredComputers { get; set; }
    } 
}
