using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Requests
{
    [Serializable]
    public class SessionUpdateRequest
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("session")]
        public SessionUpdate Session { get; set; }

        public SessionUpdateRequest()
        {
            Type = "session.update";
        }
    }
}
