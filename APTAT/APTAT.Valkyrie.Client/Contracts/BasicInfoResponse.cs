using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Valkyrie.WebApiProvider.Enums;

namespace Valkyrie.WebApiProvider.Contracts
{
    public class BasicInfoResponse : IBasicInfoResponse
    {
        [JsonProperty("first_analysis_date")]
        public DateTime? FirstAnalysisDate { get; set; }
        [JsonProperty("last_analysis_date")]
        public DateTime? LastAnalysisDate { get; set; }
        [JsonProperty("malware_probability")]
        public float? MalwareProbability { get; set; }
        /// <summary>
        /// meaning file is analyzed/not analyzed by FVS before
        /// </summary>
        [JsonProperty("presence")]
        public bool Presence { get; set; }
        /// <summary>
        /// meaning is file needs to be uploaded to FVS)
        /// "1" means upload file to Valkyrie. "0" means don't upload file to Valkyrie.
        /// </summary>
        [JsonProperty("upload")]
        public bool Upload { get; set; }
        [JsonProperty("return_code")]
        public ErrorCodesEnum ReturnCode { get; set; }

        [JsonProperty("result_description")]
        public string ResultDescription {get;set;}

        [JsonProperty("result_message")]
        public string ResultMessage { get; set; }

        [JsonProperty("sha1")]
        public string SHA1 { get; set; }
        [JsonProperty("submit_token")]
        public string SubmitToken { get; set; }
        
        [JsonProperty("verdict")]
        public ValkyrieVerdict Verdict { get; set; }
        
        [JsonProperty("verdict_source")]
        public string VerdictSource { get; set; }

        [JsonProperty("white_listed")]
        public bool IsWhiteListed { get; set; }

        [JsonProperty("category_ids")]
        public FileCategories[] CategoryIds { get; set; }
 
        [JsonProperty("filesystem_permissions")]
        public Dictionary<String, String> FilesystemPermissions { get; set; }
       
    }
}
