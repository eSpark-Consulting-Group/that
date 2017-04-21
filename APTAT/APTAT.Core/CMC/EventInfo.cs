using System.Collections.Generic;
using Newtonsoft.Json;

namespace APTAT.Core.CMC
{
    public interface IEventInfo
    {
        string EventGroupIdentifier { get; set; }
        string EventIdentifier { get; set; }
        List<IParameter> EventValues { get; set; }
    }

    public class EventInfo : IEventInfo
    {
        public EventInfo()
        {
            EventValues = new List<IParameter>();
        }

        [JsonProperty("event_group_identifier")]
        public string EventGroupIdentifier { get; set; }

        [JsonProperty("event_identifier")]
        public string EventIdentifier { get; set; }

        [JsonProperty("values_array")]
        public List<IParameter> EventValues { get; set; }
    }
}
