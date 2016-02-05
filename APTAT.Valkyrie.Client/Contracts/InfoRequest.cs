using System;
using Newtonsoft.Json;

namespace Valkyrie.WebApiProvider.Contracts
{
    public class InfoRequest : RequestBase
    {
        [JsonProperty("sha1")]
        public string SHA1 { get; set;}

        [JsonProperty("endpoint_id")]
        public Guid EndpointId{ get; set; }
    }
}
