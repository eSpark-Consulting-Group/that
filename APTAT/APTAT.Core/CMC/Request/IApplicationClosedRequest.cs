namespace APTAT.Core.CMC.Request
{
    public interface IApplicationClosedRequest:IRequest
    {
        string SId { get; set; }
    }
}