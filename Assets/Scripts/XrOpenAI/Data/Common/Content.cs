using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Common
{
    [Serializable]
    public class Content
    {
        [JsonProperty("audio")]
        public string Audio { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("transcript")]
        public string Transcript { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
