using System;
using Newtonsoft.Json;
using Valkyrie.WebApiProvider.Enums;

namespace Valkyrie.WebApiProvider.Contracts
{
    public class BasicInfoResponse
    {
        [JsonProperty("first_analysis_date")]
        public DateTime? FirstAnalysisDate { get; set; }
        [JsonProperty("last_analysis_date")]
        public DateTime? LastAnalysisDate { get; set; }
        [JsonProperty("malware_probability")]
        public float MalwareProbability { get; set; }
        /// <summary>
        /// meaning file is analyzed/not analyzed by FVS before
        /// </summary>
        [JsonProperty("presence")]
        public bool Presence { get; set; }
        [JsonProperty("return_code")]
        public ErrorCodesEnum ReturnCode { get; set; }
        [JsonProperty("sha1")]
        public string SHA1 { get; set; }
        [JsonProperty("submit_token")]
        public string SubmitToken { get; set; }
        [JsonProperty("verdict")]
        public InfoVerdictEnum Verdict { get; set; }
        [JsonProperty("white_listed")]
        public bool IsWhiteListed { get; set; }
       
    }
}
