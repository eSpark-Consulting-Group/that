namespace APTAT.Core.CMC
{
    public interface IRequestParameters
    {
        string SessionIdentifier { get; set; }
        string AnalizeIdentifier { get; set; }
        string AccountIdentifier { get; set; }
        string EventParamValue { get; set; }
        string EventId { get; set; }
        string EventGroupId { get; set; }
    }

    public class RequestParameters : IRequestParameters
    {
        public string SessionIdentifier { get; set; }
        public string AnalizeIdentifier { get; set; }
        public string AccountIdentifier { get; set; }
        public string EventParamValue { get; set; }
        public string EventId { get; set; }
        public string EventGroupId { get; set; }
    }
}