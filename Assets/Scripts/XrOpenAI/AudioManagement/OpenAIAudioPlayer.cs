using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace XrOpenAI.AudioManagement
{
    public class OpenAIAudioPlayer : MonoBehaviour
    {
        private const int OPENAI_SAMPLE_RATE = 24000;
        private const int CHANNELS = 1;
        private const int BUFFER_SIZE_SECONDS = 5;

        private AudioSource _audioSource;
        private ConcurrentQueue<float[]> _audioQueue = new ConcurrentQueue<float[]>();
        private bool _isPlaying = false;
        private bool _isPaused = false;
        private bool _isResponseComplete = false;
        private CancellationTokenSource _cancellationTokenSource;
        
        private float[] _streamBuffer;
        private int _streamBufferPosition = 0;
        private int _outputSampleRate;
        private int _bufferSize;

        public void PlayAudio(string base64AudioData)
        {
            try
            {
                _isResponseComplete = false;
                
                byte[] audioBytes = Convert.FromBase64String(base64AudioData);
                float[] audioData = ConvertBytesToFloats(audioBytes);
                
                _audioQueue.Enqueue(audioData);

                if (!_isPlaying && !_isPaused)
                {
                    StartPlayback();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void StartPlayback()
        {
            if (_isPlaying) return;

            if (_audioSource == null) return;

            _isPlaying = true;
            _isPaused = false;
            _isResponseComplete = false;

            _audioQueue = new ConcurrentQueue<float[]>();

            AudioClip streamClip = AudioClip.Create(
                "RealtimeStream",
                _bufferSize,
                CHANNELS,
                _outputSampleRate,
                true,
                OnAudioRead,
                OnAudioSetPosition
            );
            _audioSource.clip = streamClip;
            _audioSource.loop = true;
            _audioSource.Play();
        }

        private void Start()
        {
            try
            {
                AudioConfiguration config = AudioSettings.GetConfiguration();

                config.sampleRate = OPENAI_SAMPLE_RATE;

                bool success = AudioSettings.Reset(config);

                _outputSampleRate = AudioSettings.outputSampleRate;
                _bufferSize = _outputSampleRate * BUFFER_SIZE_SECONDS;

                _audioSource = gameObject.GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    _audioSource = CreateAudioSource();
                }

                _streamBuffer = new float[_bufferSize];
                _streamBufferPosition = 0;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        private AudioSource CreateAudioSource()
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1.0f;
            audioSource.pitch = 1.0f;
            return audioSource;
        }

        public void OnResponseComplete()
        {
            _isResponseComplete = true;
        }

        private float[] ConvertBytesToFloats(byte[] bytes)
        {
            int sampleCount = bytes.Length / 2;
            float[] floats = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                short sample = (short)(bytes[i * 2] | (bytes[i * 2 + 1] << 8));
                floats[i] = sample / 32768.0f;
            }

            return floats;
        }

        private void OnAudioRead(float[] data)
        {
            if (!_isPlaying || _isPaused)
            {
                Array.Clear(data, 0, data.Length);
                return;
            }

            int dataIndex = 0;

            while (dataIndex < data.Length)
            {
                if (_streamBufferPosition > 0)
                {
                    int samplesAvailable = _streamBufferPosition;
                    int samplesToRead = Mathf.Min(samplesAvailable, data.Length - dataIndex);

                    Array.Copy(_streamBuffer, 0, data, dataIndex, samplesToRead);
                    dataIndex += samplesToRead;

                    if (samplesToRead < _streamBufferPosition)
                    {
                        Array.Copy(_streamBuffer, samplesToRead, _streamBuffer, 0, _streamBufferPosition - samplesToRead);
                        _streamBufferPosition -= samplesToRead;
                    }
                    else
                    {
                        _streamBufferPosition = 0;
                    }
                }
                else
                {
                    if (_audioQueue.TryDequeue(out float[] audioData))
                    {
                        int spaceAvailable = _bufferSize - _streamBufferPosition;
                        int samplesToAdd = Mathf.Min(audioData.Length, spaceAvailable);

                        Array.Copy(audioData, 0, _streamBuffer, _streamBufferPosition, samplesToAdd);
                        _streamBufferPosition += samplesToAdd;

                        if (samplesToAdd < audioData.Length)
                        {
                            float[] remainingData = new float[audioData.Length - samplesToAdd];
                            Array.Copy(audioData, samplesToAdd, remainingData, 0, remainingData.Length);
                            _audioQueue.Enqueue(remainingData);
                        }
                    }
                    else
                    {
                        Array.Clear(data, dataIndex, data.Length - dataIndex);
                        
                        if (_isResponseComplete && _audioQueue.IsEmpty && _streamBufferPosition == 0)
                        {
                            _isPlaying = false;
                        }
        
                        break;
                    }
                }
            }
        }

        private void OnAudioSetPosition(int newPosition)
        {
            _streamBufferPosition = 0;
        }

        public void Pause()
        {
            _isPaused = true;
            if (_audioSource != null)
            {
                _audioSource.Pause();
            }
        }

        public void Resume()
        {
            if (_isPaused)
            {
                _isPaused = false;
                if (_audioSource != null)
                {
                    _audioSource.UnPause();
                }
            }
        }

        public void Stop()
        {
            _isPlaying = false;
            _isPaused = false;

            if (_audioSource != null)
            {
                _audioSource.Stop();
            }

            while (_audioQueue.TryDequeue(out _)) { }
            _streamBufferPosition = 0;
        }

        public void Dispose()
        {
            Stop();
            
            if (_audioSource != null && _audioSource.clip != null)
            {
                Destroy(_audioSource.clip);
                _audioSource.clip = null;
            }
        }

        void OnDestroy()
        {
            Dispose();
        }
    }
}
