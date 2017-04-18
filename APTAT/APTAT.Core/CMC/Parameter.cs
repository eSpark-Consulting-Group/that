using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace APTAT.Core.CMC
{
    public class Parameter
    {
        [IgnoreDataMember]
        public bool IsApplication { get; set; } = false;
        [JsonProperty("param_name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
