using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using XrOpenAI.Data.Common;

namespace XrOpenAI.Data.Events.Response
{
    [Serializable]
    public class ResponseOutput
    {
        [JsonProperty("approval_request_id")]
        public string ApprovalRequestId { get; set; }

        [JsonProperty("approve")]
        public bool? Approve { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("server_label")]
        public string ServerLabel { get; set; }

        [JsonProperty("arguments")]
        public string Arguments { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("call_id")]
        public string CallId { get; set; }

        [JsonProperty("output")]
        public string Output { get; set; }

        [JsonProperty("content")]
        public List<Content> Content { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("object")]
        public string ObjectType { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
