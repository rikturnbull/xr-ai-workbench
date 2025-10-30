using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Common
{
    [Serializable]
    public class Prompt
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("variables")]
        public Dictionary<string, string> Variables { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
