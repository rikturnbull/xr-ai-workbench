using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Common
{
    [Serializable]
    public class Tool
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
                
        [JsonProperty("parameters")]
        public Parameter Parameters { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
        
    }
}
