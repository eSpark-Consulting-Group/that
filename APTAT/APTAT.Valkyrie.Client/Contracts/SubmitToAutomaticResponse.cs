using Newtonsoft.Json;
using Valkyrie.WebApiProvider.Enums;

namespace Valkyrie.WebApiProvider.Contracts
{
    public class ValkyrieResponse
    {
        [JsonProperty("result_description")]
        public string ResultDescription { get; set; }

        [JsonProperty("result_message")]
        public string ResultMessage { get; set; }

        [JsonProperty("return_code")]
        public ErrorCodesEnum ReturnCode { get; set; }

    }

    public class SubmitToAutomaticResponse: ValkyrieResponse
    {
        [JsonProperty("analysis_id")]
        public string AnalysisId { get; set; }
    }
}
