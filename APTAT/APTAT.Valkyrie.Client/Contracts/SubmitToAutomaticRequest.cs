using System;
using Newtonsoft.Json;

namespace Valkyrie.WebApiProvider.Contracts
{
    public class SubmitToAutomaticRequest: RequestBase
    {
        [JsonProperty("submit_token")]
        public string SubmitToken { get; set; }


        [JsonProperty("file_path")]
        public string[] FilePath { get; set; }


        [JsonProperty("endpoint_id")]
        public string EndpointId { get; set; }

        [JsonProperty("apt_scan_id")]
        public string AptScanId { get; set; }        
        
    }
}
