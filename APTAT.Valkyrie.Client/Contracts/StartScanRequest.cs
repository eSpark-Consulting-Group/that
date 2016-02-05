using Newtonsoft.Json;

namespace APTAT.Valkyrie.Client
{
    public class StartScanRequest
    {
        public StartScanRequest(string apiKey)
        {
            ApiKey = apiKey;
        }

        [JsonProperty("api_key")]
        public string ApiKey { get; private set; }
    }
}