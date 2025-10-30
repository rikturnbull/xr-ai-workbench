using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using XrOpenAI.Data.Common;

namespace XrOpenAI.Data.Events.Session
{
    [Serializable]
    public class RealtimeSession
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("Audio")]
        public Audio Audio { get; set; }

        [JsonProperty("include")]
        public List<string> Include { get; set; }

        [JsonProperty("instructions")]
        public string Instructions { get; set; }

        [JsonProperty("max_output_tokens")]
        public object MaxOutputTokens { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("output_modalities")]
        public List<string> OutputModalities { get; set; }

        [JsonProperty("prompt")]
        public Prompt Prompt { get; set; }

        [JsonProperty("tool_choice")]
        public object ToolChoice { get; set; }

        [JsonProperty("tools")]
        public List<object> Tools { get; set; }

        [JsonProperty("tracing")]
        public object Tracing { get; set; }

        [JsonProperty("truncation")]
        public object Truncation { get; set; }
    }
}
