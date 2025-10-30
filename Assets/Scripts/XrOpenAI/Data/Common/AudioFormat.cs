using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Common
{
    [Serializable]
    public class AudioFormat
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("rate")]
        public int? Rate { get; set; }
    }
}
