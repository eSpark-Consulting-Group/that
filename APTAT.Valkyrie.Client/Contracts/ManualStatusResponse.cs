using Newtonsoft.Json;
using Valkyrie.WebApiProvider.Enums;

namespace Valkyrie.WebApiProvider.Contracts
{

    public class ManualStatusResponse
    {
        [JsonProperty("analysis_detail")]
        public string AnalysisDetail { get; set; }

        [JsonProperty("analysis_result_attachment_link")]
        public string AnalysisResultAttachmentLink { get; set; }

        [JsonProperty("analysis_status")]
        public AnalysisStatusEnum AnalysisStatus { get; set; }

        [JsonProperty("analysis_verdict")]
        public ValkyrieVerdict AnalysisVerdict { get; set; }

        [JsonProperty("manual_analysis_id")]
        public string ManualAnalysisId { get; set; }

        [JsonProperty("return_code")]
        public ErrorCodesEnum ReturnCode { get; set; }
        public string sha1 { get; set; }
    }

}
