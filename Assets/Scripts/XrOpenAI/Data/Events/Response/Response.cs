using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using XrOpenAI.Data.Common;

namespace XrOpenAI.Data.Events.Response
{
    [Serializable]
    public class Response
    {
        [JsonProperty("audio")]
        public Audio Audio { get; set; }

        [JsonProperty("conversation_id")]
        public string ConversationId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("max_output_tokens")]
        public object MaxOutputTokens { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("output")]
        public List<ResponseOutput> Output { get; set; }

        [JsonProperty("output_modalities")]
        public List<string> OutputModalities { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("usage")]
        public Usage Usage { get; set; }
    }
}
