using System.Collections.Generic;
using Newtonsoft.Json;

namespace APTAT.Core.CMC
{
    public class GeneralInfo : IGeneralInfo
    {
        [JsonProperty("application_identifier")]
        public string ApplicationIdentifier { get; set; }

        [JsonProperty("machine_code")]
        public string MachineCode { get; set; }

        [JsonProperty("global_machine_code")]
        public string GlobalMachineCode { get; set; }

        [JsonProperty("product_identifier")]
        public string ProductIdentifier { get; set; }

        [JsonProperty("application_values_array")]
        public List<IParameter> Values { get; set; }

        [JsonProperty("events")]
        public List<IEventInfo> EventsInfo { get; set; }

        public GeneralInfo(string applicationIdentifier,string machineCode,
                           string globalMachineCode,string productIdentifier) 
        {
            ApplicationIdentifier = applicationIdentifier;
            MachineCode = machineCode;
            GlobalMachineCode = globalMachineCode;
            ProductIdentifier = productIdentifier;

            EventsInfo = new List<IEventInfo>();
            Values = new List<IParameter>();
        }
    }
}
