using Newtonsoft.Json;

namespace Valkyrie.WebApiProvider.Contracts
{
    public class RequestBase
    {        

        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        
    }
}
