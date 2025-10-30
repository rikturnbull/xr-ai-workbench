using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Common
{
    [Serializable]
    public class NoiseReduction
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
