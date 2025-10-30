using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Requests
{
    [Serializable]
    public class InputAudioBufferAppendRequest
    {
        [JsonProperty("audio")]
        public string Audio { get; set; }

        [JsonProperty("event_id")]
        public string EventId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
        

        public InputAudioBufferAppendRequest()
        {
            Type = "input_audio_buffer.append";
        }

        public InputAudioBufferAppendRequest(string audio)
        {
            Type = "input_audio_buffer.append";
            Audio = audio;
        }
    }
}
