using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace APTAT.Valkyrie.Client.Contracts
{
    public class SubmitToAutomaticRequest: RequestBase
    {
        [JsonProperty("submit_token")]
        public string SubmitToken { get; set; }
              

        [JsonProperty("file_path")]
        public string FilePath { get; set; }


        [JsonProperty("endpoint_id")]
        public Guid EndpointId { get; set; }
    }
}
