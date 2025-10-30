using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Common
{
    [Serializable]
    public class Transcription
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("prompt")]
        public string Prompt { get; set; }
    }
}
