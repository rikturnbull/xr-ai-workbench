using System;
using Newtonsoft.Json;

namespace XrOpenAI.Data.Common
{
    [Serializable]
    public class AudioInput
    {
        [JsonProperty("format")]
        public AudioFormat Format { get; set; }

        [JsonProperty("noise_reduction")]
        public NoiseReduction NoiseReduction { get; set; }

        [JsonProperty("transcription")]
        public Transcription Transcription { get; set; }

        [JsonProperty("turn_detection")]
        public TurnDetection TurnDetection { get; set; }

        [JsonProperty("voice")]
        public string Voice { get; set; }
    }
}
