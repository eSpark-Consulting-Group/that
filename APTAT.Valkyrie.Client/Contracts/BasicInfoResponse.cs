using APTAT.Valkyrie.Client.Enums;
using Newtonsoft.Json;
using System;

namespace APTAT.Valkyrie.Client.Contracts
{
    public class BasicInfoResponse
    {
        [JsonProperty("first_analysis_date")]
        public DateTime FirstAnalysisDate { get; }
        [JsonProperty("last_analysis_date")]
        public DateTime LastAnalysisDate { get; }
        [JsonProperty("malware_probability")]
        public float MalwareProbability { get; }
        /// <summary>
        /// meaning file is analyzed/not analyzed by FVS before
        /// </summary>
        [JsonProperty("presence")]
        public bool Presence { get; }
        [JsonProperty("return_code")]
        public int ReturnCode { get; }
        [JsonProperty("sha1")]
        public string SHA1 { get; }
        [JsonProperty("submit_token")]
        public string SubmitToken { get; set; }
        [JsonProperty("verdict")]
        public VerdictEnum Verdict { get; set; }
        [JsonProperty("white_listed")]
        private bool WhiteListed { get; set; }
    }
}
