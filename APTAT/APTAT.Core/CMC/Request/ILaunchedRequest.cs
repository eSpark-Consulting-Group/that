namespace APTAT.Core.CMC.Request
{
    public interface ILaunchedRequest : IRequest
    {
        string AcId { get; set; }
        string ProductVersion { get; set; }
        string ProductIdentifier { get; set; }
    }
}