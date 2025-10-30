using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Events.Session
{
    [Serializable]
    public class SessionCreatedEvent : BaseEvent
    {
        [JsonProperty("session")]
        public Session Session { get; set; }
    }
}
