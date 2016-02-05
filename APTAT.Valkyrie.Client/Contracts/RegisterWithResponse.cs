using Newtonsoft.Json;

namespace Valkyrie.WebApiProvider.Contracts
{
    public class RegisterWithResponse
    {

     
            [JsonProperty("api_key")]
            public string ApiKey { get; set; }

            [JsonProperty("result_description")]
            public string ResultDescription { get; set; }

            [JsonProperty("result_message")]
            public string ResultMessage { get; set; }

            [JsonProperty("return_code")]
            public int ReturnCode { get; set; }
     

    }
}
