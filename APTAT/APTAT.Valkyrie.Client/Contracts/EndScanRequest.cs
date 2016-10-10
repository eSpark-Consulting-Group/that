using Newtonsoft.Json;

namespace Valkyrie.WebApiProvider.Contracts
{
    public class EndScanRequest
    {
        public EndScanRequest(string apiKey, int machineCount, string aptScanId)
        {
         
            MachineCount = machineCount;
            ApiKey = apiKey;
            AptScanId = aptScanId;
        }

        [JsonProperty("machine_count")]
        public int MachineCount { get;}

        [JsonProperty("api_key")]
        public string ApiKey { get;  }

        [JsonProperty("apt_scan_id")]
        public string AptScanId { get;  }
    }
}