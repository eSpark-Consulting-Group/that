using APTAT.Valkyrie.Client.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Valkyrie.Client.Contracts
{
    public class SubmitToManualResponse
    {
        [JsonProperty("manual_analysis_id")]
        public string ManualAnalysisId { get; set; }

        [JsonProperty("return_code")]
        public ErrorCodesEnum  ReturnCode { get; set; }

        [JsonProperty("result_message")]
        public string ResultMessage { get; set; }

        [JsonProperty("result_description")]
        public string ResultDescription { get; set; }

        [JsonProperty("sha1")]
        public string SHA1 { get; set; }
    }
}
