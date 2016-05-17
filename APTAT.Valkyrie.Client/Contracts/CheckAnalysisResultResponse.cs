using System;
using Newtonsoft.Json;
using Valkyrie.WebApiProvider.Enums;

namespace Valkyrie.WebApiProvider.Contracts
{
    public class CheckAnalysisResultResponse
    {

        [JsonProperty("additional_file_info")]
        public AdditionalFileInfo AdditionalFileInfo { get; set; }

        [JsonProperty("basic_file_info")]
        public BasicFileInfo BasicFileInfo { get; set; }

        [JsonProperty("behavioral_info")]
        public BehavioralInfo BehavioralInfo { get; set; }

        [JsonProperty("component_analysis_results")]
        public ComponentAnalysisResults ComponentAnalysisResults { get; set; }

        [JsonProperty("first_analysis_date")]
        public DateTime? FirstAnalysisDate { get; set; }

        [JsonProperty("last_analysis_date")]
        public DateTime? LastAnalysisDate { get; set; }

        [JsonProperty("malware_probability")]
        public float MalwareProbability { get; set; }

        [JsonProperty("number_of_seen_before")]
        public int? NumberOfSeenBefore { get; set; }

        [JsonProperty("return_code")]
        public ErrorCodesEnum ReturnCode { get; set; }

        [JsonProperty("sha1")]
        public string SHA1 { get; set; }

        [JsonProperty("verdict")]
        public ValkyrieVerdict Verdict { get; set; }

        [JsonProperty("verdict_source")]
        public string VerdictSource { get; set; }

        [JsonProperty("white_listed")]
        public bool IsWhiteListed { get; set; }


        [JsonProperty("category_ids")]
        public FileCategories[] CategoryIds { get; set; }
        
        public override string ToString()
        {
            return String.Format("SHA1: {0} Verdict: {1}", BasicFileInfo.SHA1, Verdict);
            
        }
    }
}