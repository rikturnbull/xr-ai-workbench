using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Common
{
    [Serializable]
    public class Parameter
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("strict")]
        public bool? Strict { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, Property> Properties { get; set; }
        
        [JsonProperty("required")]
        public List<string> Required { get; set; }
    }
}
