using System;
using System.Threading.Tasks;
using UnityEngine;

namespace XrOpenAI.AudioManagement
{
    public class OpenAIRealtimeAudioManager: MonoBehaviour
    {
        private OpenAIAudioRecorder _audioRecorder;
        private OpenAIAudioPlayer _audioPlayer;
        public Func<string, Task> onAudioData;

        public void Start()
        {
            GameObject openAiAudioPlayerGameObject = new("OpenAIAudioPlayer");
            openAiAudioPlayerGameObject.transform.SetParent(transform);
            _audioPlayer = openAiAudioPlayerGameObject.AddComponent<OpenAIAudioPlayer>();

            GameObject openAiAudioRecorderGameObject = new("OpenAIAudioRecorder");
            openAiAudioRecorderGameObject.transform.SetParent(transform);
            _audioRecorder = openAiAudioRecorderGameObject.AddComponent<OpenAIAudioRecorder>();
            _audioRecorder.Initialize(OnAudioData);
        }

        public bool StartAudioRecording(string microphoneDevice = null)
        {
            if (_audioRecorder != null && _audioRecorder.IsRecording()) return true;
            return _audioRecorder.StartRecording(microphoneDevice);
        }

        private async Task OnAudioData(string audioBase64)
        {
            await onAudioData?.Invoke(audioBase64);
        }

        public void OnResponseComplete()
        {
            _audioPlayer.OnResponseComplete();
        }

        public void StopCurrentAudioPlayback()
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Stop();
            }
        }

        public void StopAudioRecording()
        {
            if (_audioRecorder != null)
            {
                _audioRecorder.StopRecording();
            }
        }

        public void PlayAudioChunk(string audioChunkBase64)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.PlayAudio(audioChunkBase64);
            }
        }

        public void Close()
        {
            StopAudioRecording();

            if (_audioRecorder != null)
            {
                _audioRecorder.Release();
            }

            if (_audioPlayer != null)
            {
                _audioPlayer.Dispose();
            }
        }

        void OnDestroy()
        {
            Close();
        }
    }
}
