using System;
using Newtonsoft.Json;

namespace Valkyrie.WebApiProvider.Contracts
{
    public class InfoRequest : RequestBase
    {
        [JsonProperty("sha1")]
        public string SHA1 { get; set;}

        [JsonProperty("endpoint_id")]
        public Guid EndpointId{ get; set; }
        
        /// <summary>
        /// Path of File on Origin, Optional
        /// </summary>
        [JsonProperty("file_path")]
        public Guid FilePath { get; set; }
    }


    public class CheckAnalysisResultRequest :RequestBase
    {
        [JsonProperty("analysis_id")]
        public string AnalysisId { get; set; }      
    }
}
