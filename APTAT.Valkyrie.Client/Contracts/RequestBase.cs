using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Valkyrie.Client.Contracts
{
    public class RequestBase
    {
        [JsonProperty("client_id")]
        public string Client_id { get; set; }

        [JsonProperty("api_key")]
        public string ApiKey { get; set; }
    }
}
