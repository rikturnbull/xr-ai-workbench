using System;
using Newtonsoft.Json;
using XrOpenAI.Data.Events.Response;

namespace XrOpenAI.Data.Events
{
    [Serializable]
    public class ResponseOutputItemDoneEvent : BaseEvent
    {
        [JsonProperty("response_id")]
        public string ResponseId { get; set; }

        [JsonProperty("output_index")]
        public int? OutputIndex { get; set; }

        [JsonProperty("item")]
        public ResponseOutput Item { get; set; }
    }
}
