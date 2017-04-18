namespace APTAT.Core.CMC.Request
{
    public interface IApplicationClosedRequest:IRequest
    {
        string Sid { get; set; }
    }
}