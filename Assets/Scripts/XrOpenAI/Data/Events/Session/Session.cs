using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Events.Session
{
    [Serializable]
    public class Session
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
