using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Events
{
    [Serializable]
    public class BaseEvent
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("event_id")]
        public string EventId { get; set; }
    }
}
