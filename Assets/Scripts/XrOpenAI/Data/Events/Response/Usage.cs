using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Events.Response
{
    [Serializable]
    public class Usage
    {
        [JsonProperty("total_tokens")]
        public int? TotalTokens { get; set; }

        [JsonProperty("input_tokens")]
        public int? InputTokens { get; set; }

        [JsonProperty("output_tokens")]
        public int? OutputTokens { get; set; }
    }
}
