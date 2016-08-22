using Newtonsoft.Json;
using Valkyrie.WebApiProvider.Enums;

namespace Valkyrie.WebApiProvider.Contracts
{
    public class SubmitToManualResponse
    {
        [JsonProperty("manual_analysis_id")]
        public string ManualAnalysisId { get; set; }

        [JsonProperty("return_code")]
        public ErrorCodesEnum  ReturnCode { get; set; }               

        [JsonProperty("result_description")]
        public string ResultDescription { get; set; }

        [JsonProperty("result_message")]
        public string ResultMessage { get; set; }

        [JsonProperty("sha1")]
        public string SHA1 { get; set; }
    }
}
