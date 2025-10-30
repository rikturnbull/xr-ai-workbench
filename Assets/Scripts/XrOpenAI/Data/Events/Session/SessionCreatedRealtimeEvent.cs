using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Events.Session
{
    [Serializable]
    public class SessionCreatedRealtimeEvent : BaseEvent
    {
        [JsonProperty("session")]
        public RealtimeSession Session { get; set; }
    }
}
