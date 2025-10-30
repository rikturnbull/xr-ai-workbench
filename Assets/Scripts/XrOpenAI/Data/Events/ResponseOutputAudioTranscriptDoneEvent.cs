using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Events
{
    [Serializable]
    public class ResponseOutputAudioTranscriptDoneEvent : BaseEvent
    {
        [JsonProperty("content_index")]
        public int? ContentIndex { get; set; }

        [JsonProperty("item_id")]
        public string ItemId { get; set; }

        [JsonProperty("output_index")]
        public int? OutputIndex { get; set; }

        [JsonProperty("response_id")]
        public string ResponseId { get; set; }

        [JsonProperty("transcript")]
        public string Transcript { get; set; }
    }
}
