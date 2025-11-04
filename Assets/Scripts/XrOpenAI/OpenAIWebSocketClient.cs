using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using XrOpenAI.Data.Events;
using XrOpenAI.Data.Events.Session;
using XrOpenAI.Data.Events.Response;

namespace XrOpenAI
{
    public class OpenAIWebSocketClient
    {
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        private WebSocketClient _webSocket;
        private bool _isConnected = false;

        public event Func<RealtimeSession, Task> onSessionCreatedRealtime;
        public event Action<RealtimeSession> onSessionUpdatedRealtime;

        public event Action onAudioSpeechStarted;
        public event Action onAudioSpeechStopped;
        public event Action onAudioBufferCommitted;

        public event Action onResponseCreated;
        public event Action<ResponseAudioDeltaEvent> onResponseAudioDelta;
        public event Action onResponseAudioDone;
        public event Action<string> onResponseAudioTranscriptDone;
        public event Action onResponseDone;

        public event Action<ResponseOutput> onResponseOutputItemDone;

        public event Action<string> onError;

        private Transform _parent;
        public OpenAIWebSocketClient(Transform parent)
        {
            _parent = parent;
        }

        private Dictionary<string, string> GetHeaders(string apiKey)
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {apiKey}" }
            };
            return headers;
        }

        public bool IsConnected => _isConnected;

        public async Task Connect(string url, string apiKey)
        {
            try
            {
                _webSocket = WebSocketClient.Create(_parent);

                _webSocket.onOpen += () =>
                {
                    _isConnected = true;
                };

                _webSocket.onMessage += async (bytes) =>
                {
                    string message = System.Text.Encoding.UTF8.GetString(bytes);
                    await HandleMessage(message);
                };

                _webSocket.onError += (errorMsg) =>
                {
                    _isConnected = false;
                    onError?.Invoke($"Connection failed: {errorMsg}");
                };

                _webSocket.onClose += (closeCode) =>
                {
                    Debug.LogWarning($"OpenAIRealtimeClient: WebSocket closed with code: {closeCode}");
                    _isConnected = false;
                };

                Dictionary<string, string> headers = GetHeaders(apiKey);
                await _webSocket.Connect(url, headers);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                onError?.Invoke($"Error: {e.Message}");
            }
        }

        private T DeserializeMessage<T>(string message)
        {
            return JsonConvert.DeserializeObject<T>(message);
        }

        private async Task HandleMessage(string message)
        {
            var baseEvent = DeserializeMessage<BaseEvent>(message);
            
            switch (baseEvent.Type)
            {
                case "session.created":
                    SessionCreatedEvent sessionCreatedEvent = DeserializeMessage<SessionCreatedEvent>(message);
                    if (sessionCreatedEvent.Session.Type == "realtime")
                    {
                        SessionCreatedRealtimeEvent sessionCreatedRealtimeEvent = DeserializeMessage<SessionCreatedRealtimeEvent>(message);
                        await onSessionCreatedRealtime?.Invoke(sessionCreatedRealtimeEvent.Session);
                    }
                    break;
                case "session.updated":
                    SessionUpdatedEvent sessionUpdatedEvent = DeserializeMessage<SessionUpdatedEvent>(message);
                    if (sessionUpdatedEvent.Session.Type == "realtime")
                    {
                        SessionUpdatedRealtimeEvent sessionUpdatedRealtimeEvent = DeserializeMessage<SessionUpdatedRealtimeEvent>(message);
                        onSessionUpdatedRealtime?.Invoke(sessionUpdatedRealtimeEvent.Session);
                    }
                    break;
                case "input_audio_buffer.speech_started":
                    onAudioSpeechStarted?.Invoke();
                    break;

                case "input_audio_buffer.speech_stopped":
                    onAudioSpeechStopped?.Invoke();
                    break;

                case "input_audio_buffer.committed":
                    onAudioBufferCommitted?.Invoke();
                    break;
                case "conversation.item.input_audio_transcription.delta":
                    break;
                case "conversation.item.input_audio_transcription.completed":
                    break;
                case "conversation.item.input_audio_transcription.failed":
                    break;
                case "response.created":
                    onResponseCreated?.Invoke();
                    break;
                case "response.output_audio.delta":
                    ResponseAudioDeltaEvent responseAudioDeltaEvent = DeserializeMessage<ResponseAudioDeltaEvent>(message);
                    onResponseAudioDelta?.Invoke(responseAudioDeltaEvent);
                    break;
                case "response.output_audio.done":
                    onResponseAudioDone?.Invoke();
                    break;
                case "response.output_audio_transcript.delta":
                    break;
                case "response.output_audio_transcript.done":
                    ResponseOutputAudioTranscriptDoneEvent responseOutputAudioTranscriptDoneEvent = DeserializeMessage<ResponseOutputAudioTranscriptDoneEvent>(message);
                    onResponseAudioTranscriptDone?.Invoke(responseOutputAudioTranscriptDoneEvent.Transcript);
                    break;
                case "response.done":
                    onResponseDone?.Invoke();
                    break;
                case "error":
                    ErrorEvent errorEvent = DeserializeMessage<ErrorEvent>(message);
                    string errorMessage = errorEvent.Error?.Message ?? "Unknown error";
                    onError?.Invoke(errorMessage);
                    break;
                case "conversation.item.added":
                    break;

                case "conversation.item.done":
                    break;
                case "response.output_item.done":
                    ResponseOutputItemDoneEvent responseOutputItemDoneEvent = DeserializeMessage<ResponseOutputItemDoneEvent>(message);
                    if (responseOutputItemDoneEvent.Item != null && responseOutputItemDoneEvent.Item.Type == "function_call")
                    {
                        onResponseOutputItemDone?.Invoke(responseOutputItemDoneEvent.Item);
                    }
                    break;
                default:
                    break;
            }
        }

        public async Task SendRequest(object request)
        {
            if (!_isConnected || _webSocket == null) return;

            try
            {
                string messageJson = JsonConvert.SerializeObject(request, jsonSettings);
                await _webSocket.SendText(messageJson);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                onError?.Invoke($"Failed to send session update: {e.Message}");
            }
        }

        public async Task Close()
        {
            if (_webSocket != null)
            {
                await _webSocket.Close();
                _webSocket = null;
            }
        }
    }
}
