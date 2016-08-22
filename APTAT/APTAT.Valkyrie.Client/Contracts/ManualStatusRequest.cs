using Newtonsoft.Json;

namespace Valkyrie.WebApiProvider.Contracts
{
    public class ManualStatusRequest:RequestBase
    {
        public ManualStatusRequest(string analysisId, string apiKey)
        {
            ManualAnalysisId = analysisId;
            this.ApiKey = apiKey;
        }

        [JsonProperty("manual_analysis_id")]
        public string ManualAnalysisId
        {
            get;
            private set;
        }
    }
}
