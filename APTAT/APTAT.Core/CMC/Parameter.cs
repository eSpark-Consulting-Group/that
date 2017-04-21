using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace APTAT.Core.CMC
{
    public interface IParameter
    {
        bool IsApplication { get; set; }
        string Name { get; set; }
        string Value { get; set; }
    }

    public class Parameter : IParameter
    {
        [IgnoreDataMember]
        public bool IsApplication { get; set; } = false;
        [JsonProperty("param_name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
