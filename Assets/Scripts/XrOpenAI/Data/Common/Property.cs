using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Common
{
    [Serializable]
    public class Property
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("items")]
        public Property Items { get; set; }
        
        [JsonProperty("properties")]
        public Dictionary<string, Property> Properties { get; set; }
    }
}
