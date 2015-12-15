using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace APTAT.Valkyrie.Client.Contracts
{
    public class RequestBase
    {        

        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        
    }
}
