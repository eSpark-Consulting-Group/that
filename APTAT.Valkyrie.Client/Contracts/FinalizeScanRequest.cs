using System;
using Newtonsoft.Json;

namespace APTAT.Valkyrie.Client.Contracts
{
    public class FinalizeScanRequest
    {
        public FinalizeScanRequest(DateTime start, DateTime end, string apiKey, int machineCount)
        {
            const string formatStr = "yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'fff''";
            StartTime = start.ToUniversalTime().ToString(formatStr);
            EndTime = end.ToUniversalTime().ToString(formatStr);
            MachineCount = machineCount;
            ApiKey = apiKey;
        }

        [JsonProperty("machine_count")]
        public int MachineCount { get; private set; }

        [JsonProperty("start_time")]
        public string StartTime { get; private set; }

        [JsonProperty("end_time")]
        public string EndTime { get; private set; }

        [JsonProperty("api_key")]
        public string ApiKey { get; private  set; }
    }
}