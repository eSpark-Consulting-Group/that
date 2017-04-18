namespace APTAT.Core.CMC.Request
{
    public interface ILaunchedRequest : IRequest
    {
        string Acid { get; }
        string ProductVersion { get; }
        string ProductIdentifier { get; }
    }
}