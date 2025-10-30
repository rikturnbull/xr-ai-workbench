using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Common
{
    [Serializable]
    public class TurnDetection
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("create_response")]
        public bool? CreateResponse { get; set; }
                        
        [JsonProperty("idle_timeout_ms")]
        public int? IdleTimeoutMs { get; set; }

        [JsonProperty("interrupt_response")]
        public bool? InterruptResponse { get; set; }
                
        [JsonProperty("prefix_padding_ms")]
        public int? PrefixPaddingMs { get; set; }
        
        [JsonProperty("silence_duration_ms")]
        public int? SilenceDurationMs { get; set; }
        
        [JsonProperty("threshold")]
        public double? Threshold { get; set; }

        [JsonProperty("eagerness")]
        public string Eagerness { get; set; }
    }
}
