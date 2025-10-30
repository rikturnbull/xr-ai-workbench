using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Requests
{
    [Serializable]
    public class ConversationItemCreateRequest
    {
        [JsonProperty("event_id")]
        public string EventId { get; set; }

        [JsonProperty("item")]
        public ConversationItem Item { get; set; }

        [JsonProperty("previous_item_id")]
        public string PreviousItemId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
        
        public ConversationItemCreateRequest()
        {
            Type = "conversation.item.create";
        }
    }
}
