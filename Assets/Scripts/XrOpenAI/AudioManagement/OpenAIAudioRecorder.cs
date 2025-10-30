using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace XrOpenAI.AudioManagement
{
    public class OpenAIAudioRecorder : MonoBehaviour
    {
        private const int SAMPLE_RATE = 24000;
        private const int RECORD_LENGTH_SEC = 1;

        // [SerializeField] private float microphoneGain = 3f;

        private AudioClip _microphoneClip;
        private string _selectedDevice;
        private bool _isRecording = false;
        private int _lastSamplePosition = 0;
        private Func<string, Task> onAudioData;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly ConcurrentQueue<Action> _queue = new();

        public void Initialize(Func<string, Task> audioDataCallback)
        {
            onAudioData = audioDataCallback;
        }

        private string GetMicrophoneDevice(string microphoneDevice = null)
        {
            if (!string.IsNullOrEmpty(microphoneDevice))
            {
                return microphoneDevice;
            }

            if (Microphone.devices.Length == 0)
            {
                Debug.LogError("No microphone devices found");
                return null;
            }

            foreach (string device in Microphone.devices)
            {
                string deviceLower = device.ToLower();
                if (deviceLower.Contains("quest") || 
                    deviceLower.Contains("oculus") || 
                    deviceLower.Contains("meta") ||
                    deviceLower.Contains("rift"))
                {
                    return device;
                }
            }

            return Microphone.devices[0];
        }

        public bool StartRecording(string microphoneDevice = null)
        {
            if (_isRecording) return true;

            try
            {
                _selectedDevice = GetMicrophoneDevice(microphoneDevice);

                _microphoneClip = Microphone.Start(_selectedDevice, true, RECORD_LENGTH_SEC, SAMPLE_RATE);

                if (_microphoneClip == null) return false;

                _isRecording = true;
                _lastSamplePosition = 0;

                _cancellationTokenSource = new CancellationTokenSource();
                _ = ProcessMicrophoneData(_cancellationTokenSource.Token);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        private async Task ProcessMicrophoneData(CancellationToken cancellationToken)
        {
            try
            {
                // Wait for microphone to start
                while (Microphone.GetPosition(_selectedDevice) <= 0 && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(10);
                }

                float[] samples = new float[SAMPLE_RATE / 10]; // 100ms chunks

                while (_isRecording && !cancellationToken.IsCancellationRequested)
                {
                    int currentPosition = Microphone.GetPosition(_selectedDevice);
                    
                    if (currentPosition < 0)
                    {
                        break;
                    }

                    int samplesAvailable;
                    if (currentPosition < _lastSamplePosition)
                    {
                        samplesAvailable = _microphoneClip.samples - _lastSamplePosition + currentPosition;
                    }
                    else
                    {
                        samplesAvailable = currentPosition - _lastSamplePosition;
                    }

                    if (samplesAvailable >= samples.Length)
                    {
                        if (_lastSamplePosition + samples.Length <= _microphoneClip.samples)
                        {
                            _microphoneClip.GetData(samples, _lastSamplePosition);
                        }
                        else
                        {
                            int firstPartLength = _microphoneClip.samples - _lastSamplePosition;
                            float[] firstPart = new float[firstPartLength];
                            float[] secondPart = new float[samples.Length - firstPartLength];
                            
                            _microphoneClip.GetData(firstPart, _lastSamplePosition);
                            _microphoneClip.GetData(secondPart, 0);
                            
                            Array.Copy(firstPart, 0, samples, 0, firstPartLength);
                            Array.Copy(secondPart, 0, samples, firstPartLength, secondPart.Length);
                        }

                        _lastSamplePosition = (_lastSamplePosition + samples.Length) % _microphoneClip.samples;

                        byte[] pcm16Data = ConvertFloatsToPCM16(samples);
                        string base64Audio = Convert.ToBase64String(pcm16Data);

                        await Enqueue(async () =>
                        {
                            await onAudioData?.Invoke(base64Audio);
                        });
                    }

                    await Task.Delay(10); // Small delay to prevent tight loop
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private byte[] ConvertFloatsToPCM16(float[] samples)
        {
            byte[] pcm16 = new byte[samples.Length * 2];

            for (int i = 0; i < samples.Length; i++)
            {
                float sample = Mathf.Clamp(samples[i], -1.0f, 1.0f);
                short pcmValue = (short)(sample * 32767f);

                pcm16[i * 2] = (byte)(pcmValue & 0xFF);
                pcm16[i * 2 + 1] = (byte)((pcmValue >> 8) & 0xFF);
            }

            return pcm16;
        }

        public void StopRecording()
        {
            if (!_isRecording) return;

            _isRecording = false;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            if (Microphone.IsRecording(_selectedDevice))
            {
                Microphone.End(_selectedDevice);
            }

            _microphoneClip = null;
            _lastSamplePosition = 0;
        }

        public bool IsRecording()
        {
            return _isRecording;
        }

        private Task Enqueue(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            _queue.Enqueue(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        void Update()
        {
            while (_queue.TryDequeue(out Action action))
            {
                action?.Invoke();
            }
        }

        public void Release()
        {
            StopRecording();
        }

        void OnDestroy()
        {
            Release();
        }
    }
}
