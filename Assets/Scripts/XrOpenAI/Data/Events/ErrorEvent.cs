using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Events
{
    [Serializable]
    public class ErrorEvent : BaseEvent
    {
        [JsonProperty("error")]
        public ErrorBody Error { get; set; }
    }

    [Serializable]
    public class ErrorBody
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("code")]
        public string Code { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("param")]
        public string Param { get; set; }
        
        [JsonProperty("event_id")]
        public string EventId { get; set; }
    }
}
