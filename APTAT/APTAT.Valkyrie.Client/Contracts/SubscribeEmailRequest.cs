using Newtonsoft.Json;

namespace Valkyrie.WebApiProvider.Contracts
{
    public class SubscribeEmailRequest : RequestBase
    {
        [JsonProperty("scan_id")]
        public string ScanId { get; set; }

        [JsonProperty("customer_email_list")]
        public string[] CustomerEmails { get; set; }

        [JsonProperty("salesman_email_list")]
        public string[] SalesmanEmails { get; set; }
    }
}