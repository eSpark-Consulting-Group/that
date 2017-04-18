using System.Collections.Generic;
using Newtonsoft.Json;

namespace APTAT.Core.CMC
{
    public class EventInfo
    {
        public EventInfo()
        {
            EventValues = new List<Parameter>();
        }

        [JsonProperty("event_group_identifier")]
        public string EventGroupIdentifier { get; set; }

        [JsonProperty("event_identifier")]
        public string EventIdentifier { get; set; }

        [JsonProperty("values_array")]
        public List<Parameter> EventValues { get; set; }
    }
}
