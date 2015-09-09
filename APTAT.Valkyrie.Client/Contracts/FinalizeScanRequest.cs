using System;
using Newtonsoft.Json;

namespace APTAT.Valkyrie.Client.Contracts
{
    public class FinalizeScanRequest
    {
        [JsonProperty("machine_count")]
        public int MachineCount { get; set; }

        [JsonProperty("start_time")]
        public string StartTime { get; set; }

        [JsonProperty("end_time")]
        public string EndTime { get; set; }

        [JsonProperty("api_key")]
        public string ApiKey { get; set; }
    }
}