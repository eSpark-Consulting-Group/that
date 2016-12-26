using System;
using Newtonsoft.Json;

namespace Valkyrie.WebApiProvider.Contracts
{
    public class InfoRequest : RequestBase
    {
        public InfoRequest(string sha1, Guid endpointId,  string aptScanId, string apiKey, string path)            
        {
            SHA1 = sha1;
            EndpointId = endpointId;
            FilePath = path;
            AptScanId = aptScanId;
            ApiKey = apiKey;                    
        }

        [JsonProperty("sha1")]
        public string SHA1 { get; }

        [JsonProperty("endpoint_id")]
        public Guid EndpointId { get; }

        /// <summary>
        /// Path of File on Origin, Optional
        /// </summary>
        [JsonProperty("file_path")]
        public string FilePath { get; }

        [JsonProperty("apt_scan_id")]
        public string AptScanId { get; }

      
        
    }


    public class CheckAnalysisResultRequest : RequestBase
    {
        public CheckAnalysisResultRequest(string analysisId, string apiKey)
        {
            this.ApiKey = apiKey;
            AnalysisId = analysisId;
        }
        [JsonProperty("analysis_id")]
        public string AnalysisId { get;  }
    }
}
