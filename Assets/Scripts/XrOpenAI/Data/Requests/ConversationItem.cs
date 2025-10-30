using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using XrOpenAI.Data.Common;

namespace XrOpenAI.Data.Requests
{
    [Serializable]
    public class ConversationItem
    {
        [JsonProperty("content")]
        public List<Content> Content { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("call_id")]
        public string CallId { get; set; }

        [JsonProperty("output")]
        public string Output { get; set; }
    }
}
