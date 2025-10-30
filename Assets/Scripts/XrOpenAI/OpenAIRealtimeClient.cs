using XrOpenAI.Data.Events;
using XrOpenAI.Data.Events.Session;
using XrOpenAI.Data.Common;
using XrOpenAI.Data.Requests;
using XrOpenAI.Data.Events.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using XrOpenAI.AudioManagement;
using System.ComponentModel;

namespace XrOpenAI
{
    public class OpenAIRealtimeClient
    {
        private const string WEBSOCKET_URL = "wss://api.openai.com/v1/realtime";
        private const string MODEL = "gpt-realtime";

        private string _systemPrompt;
        private string _voice;

        public event Action<string> onError;
        public event Action onReady;
        public event Action<string> onStatus;
        public event Action<string> onTranscription;

        private OpenAIRealtimeAudioManager _audioManager;
        private OpenAIWebSocketClient _openAiWebSocketClient;

        private Dictionary<string, Action<string, string>> _functionHandlers = null;
        private List<Tool> _tools;

        private string _microphoneDevice;

        public OpenAIRealtimeClient(Transform parent, string microphoneDevice = null)
        {
            _audioManager = CreateAudioManager(parent);
            _openAiWebSocketClient = CreateWebSocketClient(parent);
            _microphoneDevice = microphoneDevice;
        }

        private OpenAIRealtimeAudioManager CreateAudioManager(Transform parent)
        {
            OpenAIRealtimeAudioManager audioManager;
            GameObject audioManagerGameObject = new("OpenAIRealtimeAudioManager");
            audioManagerGameObject.transform.SetParent(parent);
            audioManager = audioManagerGameObject.AddComponent<OpenAIRealtimeAudioManager>();
            audioManager.onAudioData += OnAudioData;
            return audioManager;
        }

        private OpenAIWebSocketClient CreateWebSocketClient(Transform parent)
        {
            OpenAIWebSocketClient openAiWebSocketClient = new(parent);

            openAiWebSocketClient.onSessionCreatedRealtime += OnSessionCreatedRealtime;
            openAiWebSocketClient.onSessionUpdatedRealtime += OnSessionUpdatedRealtime;
            openAiWebSocketClient.onAudioSpeechStarted += OnAudioSpeechStarted;
            openAiWebSocketClient.onAudioSpeechStopped += OnAudioSpeechStopped;
            openAiWebSocketClient.onAudioBufferCommitted += OnAudioBufferCommitted;
            openAiWebSocketClient.onResponseCreated += OnResponseCreated;
            openAiWebSocketClient.onResponseAudioDelta += OnResponseAudioDelta;
            openAiWebSocketClient.onResponseAudioDone += OnResponseAudioDone;
            openAiWebSocketClient.onResponseAudioTranscriptDone += OnResponseAudioTranscriptDone;
            openAiWebSocketClient.onResponseDone += OnResponseDone;
            openAiWebSocketClient.onResponseOutputItemDone += OnResponseOutputItemDone;
            openAiWebSocketClient.onError += onError;

            return openAiWebSocketClient;
        }

        private string GetUrl(string url, string model)
        {
            return $"{url}?model={model}";
        }

        public async Task<bool> Connect(string apiKey, string systemPrompt, string voice, List<Tool> tools = null, Dictionary<string, Action<string, string>> functionHandlers = null)
        {
            try
            {
                Debug.Log("OpenAIRealtimeClient:Connecting to OpenAI Realtime WebSocket...");
                SetContext(systemPrompt, voice, tools, functionHandlers);
                Debug.Log("OpenAIRealtimeClient: Connecting to WebSocket URL: " + GetUrl(WEBSOCKET_URL, MODEL));
                await _openAiWebSocketClient.Connect(GetUrl(WEBSOCKET_URL, MODEL), apiKey);
                Debug.Log("OpenAIRealtimeClient: Connected to OpenAI Realtime WebSocket");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                onError?.Invoke($"Connection failed: {e.Message}");
                return false;
            }
        }

        private void SetContext(string systemPrompt, string voice, List<Tool> tools = null, Dictionary<string, Action<string, string>> functionHandlers = null)
        {
            _functionHandlers = functionHandlers;
            _systemPrompt = systemPrompt;
            _voice = voice;
            _tools = tools;
        }

        private async Task OnSessionCreatedRealtime(RealtimeSession sessionCreatedRealtime)
        {
            onStatus?.Invoke("Session created");
            try
            {
                await SendSessionUpdate();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                onError?.Invoke($"Failed to send session update: {e.Message}");
            }
        }

        private void OnSessionUpdatedRealtime(RealtimeSession sessionUpdatedRealtime)
        {
            try
            {
                if (_audioManager.StartAudioRecording(_microphoneDevice))
                {
                    onStatus?.Invoke("Ready - Speak now!");
                    onReady?.Invoke();
                }
                else
                {
                    onError?.Invoke("Failed to start audio recording");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                onError?.Invoke($"Failed to start audio recording: {e.Message}");
            }
        }

        // public async Task CancelCurrentResponse()
        // {
        //     try
        //     {
        //         var cancelRequest = new
        //         {
        //             type = "response.cancel"
        //         };
        //         await _openAiWebSocketClient.SendRequest(cancelRequest);
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogException(e);
        //     }
        // }
       
        private void OnAudioSpeechStarted()
        {
            _audioManager.StopCurrentAudioPlayback();
            // _ = CancelCurrentResponse();
            onStatus?.Invoke("Listening...");
        }

        private void OnAudioSpeechStopped()
        {
            onStatus?.Invoke("Processing...");
        }

        private void OnAudioBufferCommitted()
        {
        }

        private void OnResponseCreated()
        {
            onStatus?.Invoke("AI responding...");
        }

        private void OnResponseAudioDelta(ResponseAudioDeltaEvent responseAudioDelta)
        {
            _audioManager.PlayAudioChunk(responseAudioDelta.Delta);
        }

        private void OnResponseAudioDone()
        {
            _audioManager.OnResponseComplete();
        }

        private void OnResponseAudioTranscriptDone(string transcript)
        {
            onTranscription?.Invoke(transcript);
        }

        private void OnResponseDone()
        {
            // onStatus?.Invoke("Ready");
        }

        private void OnResponseOutputItemDone(ResponseOutput responseOutput)
        {
            if (_functionHandlers == null) return;
            if (_functionHandlers.TryGetValue(responseOutput.Name, out var handler))
            {
                handler?.Invoke(responseOutput.CallId, responseOutput.Arguments);
            }
        }

        private async Task SendSessionUpdate()
        {
            SessionUpdateRequest request = new()
            {
                Session = new SessionUpdate
                {
                    Type = "realtime",
                    Audio = new Audio
                    {
                        Output = new AudioOutput
                        {
                            Voice = _voice
                        }
                    },
                    Instructions = _systemPrompt,
                    Model = MODEL,
                    Tools = _tools,
                    ToolChoice = "auto"
                }
            };
            await _openAiWebSocketClient.SendRequest(request);
        }

        public async Task SendFunctionCallResult(string callId, string output)
        {
            ConversationItemCreateRequest request = new()
            {
                Item = new ConversationItem
                {
                    Type = "function_call_output",
                    CallId = callId,
                    Output = output
                }
            };
            await _openAiWebSocketClient.SendRequest(request);
        }

        public async Task SendConversationItem(string message, string previousItemId = null)
        {
            ConversationItemCreateRequest request = new()
            {
                Item = new ConversationItem
                {
                    Type = "message",
                    Role = "user",
                    Content = new List<Content>
                    {
                        new() {
                            Type = "input_text",
                            Text = message
                        }
                    }
                },
                PreviousItemId = previousItemId
            };
            await _openAiWebSocketClient.SendRequest(request);
        }

        private async Task OnAudioData(string audioBase64)
        {
            try
            {
                InputAudioBufferAppendRequest request = new(audioBase64);
                await _openAiWebSocketClient.SendRequest(request);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                onError?.Invoke($"Failed to send audio data: {e.Message}");
            }
        }

        private void StopAudioRecording()
        {
            if (_audioManager != null)
            {
                _audioManager.StopAudioRecording();
                onStatus?.Invoke("Audio recording stopped");
            }
        }

        public async Task Close()
        {
            StopAudioRecording();

            if (_openAiWebSocketClient != null)
            {
                await _openAiWebSocketClient.Close();
                _openAiWebSocketClient = null;
            }

            if (_audioManager != null)
            {
                _audioManager.Close();
                _audioManager = null;
            }
        }
    }
}
