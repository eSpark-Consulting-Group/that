using Newtonsoft.Json;

namespace APTAT.Valkyrie.Client.Contracts
{
    public class EndScanRequest
    {
        public EndScanRequest(string apiKey, int machineCount)
        {
         
            MachineCount = machineCount;
            ApiKey = apiKey;
        }

        [JsonProperty("machine_count")]
        public int MachineCount { get; private set; }

        [JsonProperty("api_key")]
        public string ApiKey { get; private  set; }
    }
}