using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace APTAT.Valkyrie.Client.Contracts
{
    public class ManualStatusRequest
    {
        [JsonProperty("manual_analysis_id")]
        public string ManualAnalysisId { get; }
    }
}
