using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Common
{
    [Serializable]
    public class AudioOutput
    {
        [JsonProperty("format")]
        public object AudioFormat { get; set; }

        [JsonProperty("speed")]
        public double? Speed { get; set; }

        [JsonProperty("voice")]
        public string Voice { get; set; }
    }
}
