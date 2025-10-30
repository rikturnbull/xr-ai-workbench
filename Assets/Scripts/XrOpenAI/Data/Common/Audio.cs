using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Common
{
    [Serializable]
    public class Audio
    {
        [JsonProperty("input")]
        public AudioInput Input { get; set; }

        [JsonProperty("output")]
        public AudioOutput Output { get; set; }
    }
}
