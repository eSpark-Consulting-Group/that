namespace APTAT.Core.CMC.Request
{
    public interface IApplicationLaunchedRequest : IRequest
    {
        string AcId { get; set; }
        string ProductVersion { get; set; }
        string ProductIdentifier { get; set; }
    }
}