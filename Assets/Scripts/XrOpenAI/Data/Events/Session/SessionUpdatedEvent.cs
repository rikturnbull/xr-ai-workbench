using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Events.Session
{
    [Serializable]
    public class SessionUpdatedEvent : BaseEvent
    {
        [JsonProperty("session")]
        public Session Session { get; set; }
    }
}
