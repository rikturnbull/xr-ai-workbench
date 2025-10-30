using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using XrOpenAI.Data.Common;

namespace XrOpenAI.Data.Requests
{
    [Serializable]
    public class SessionUpdate
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("audio")]
        public Audio Audio { get; set; }

        [JsonProperty("include")]
        public List<string> Include { get; set; }

        [JsonProperty("instructions")]
        public string Instructions { get; set; }

        [JsonProperty("max_output_tokens")]
        public int? MaxOutputTokens { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("output_modalities")]
        public List<string> OutputModalities { get; set; }

        [JsonProperty("prompt")]
        public Prompt Prompt { get; set; }

        [JsonProperty("tool_choice")]
        public string ToolChoice { get; set; }

        [JsonProperty("tools")]
        public List<Tool> Tools { get; set; }

        [JsonProperty("tracing")]
        public string Tracing { get; set; }

        [JsonProperty("truncation")]
        public double? Truncation { get; set; }
    }
}
