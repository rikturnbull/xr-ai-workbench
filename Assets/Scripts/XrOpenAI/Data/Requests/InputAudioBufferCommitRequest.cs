using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Requests
{
    [Serializable]
    public class InputAudioBufferCommitRequest
    {
        [JsonProperty("event_id")]
        public string EventId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        public InputAudioBufferCommitRequest()
        {
            Type = "input_audio_buffer.commit";
        }
    }
}
