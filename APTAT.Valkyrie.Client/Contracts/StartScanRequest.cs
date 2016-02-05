using Newtonsoft.Json;

namespace Valkyrie.WebApiProvider.Contracts
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