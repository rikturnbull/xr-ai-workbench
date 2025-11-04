using XrOpenAI;
using XrOpenAI.Data.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using XrAiAccelerator;

public class OpenAILive : MonoBehaviour
{
    enum VOICE { alloy, ash, ballad, coral, echo, sage, shimmer, verse, marin, cedar }

    [Header("OpenAI Configuration")]
    [TextArea(3, 10)][SerializeField] private string _systemPrompt = "You are a helpful assistant.";
    [SerializeField] private VOICE _voice = VOICE.alloy;

    [Header("UI/Debug")]
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private TextMeshProUGUI _agentTranscriptText;
    [SerializeField] private bool _autoConnectOnStart = false;

    [Header("Audio")]
    [HideInInspector][SerializeField] private int _selectedMicrophoneIndex = 0;

    private Dictionary<string, MenuController> _menuControllers = new Dictionary<string, MenuController>();
    private OpenAIRealtimeClient _openAiRealTimeClient;
    // private bool _isConnected = false;
    private XrAiSecretsManager _secretsManager;

    private IEnumerator Start()
    {
        _secretsManager = XrAiSecretsManager.GetSecretsManager();

        _openAiRealTimeClient = new OpenAIRealtimeClient(transform, GetMicrophone());
        _openAiRealTimeClient.onError += OnError;
        _openAiRealTimeClient.onStatus += OnStatus;
        _openAiRealTimeClient.onReady += OnReady;
        _openAiRealTimeClient.onTranscription += OnTranscription;

        FindMenuControllers();

        OnStatus("Initialized. Ready to connect.");

        if (_autoConnectOnStart)
        {
            Task task = Connect();
            yield return new WaitUntil(() => task.IsCompleted);
        }
    }

    private void FindMenuControllers()
    {
        foreach(var workflow in new string[] { "ImageTo3D_Button", "ImageToImage_Button", "TextToImage_Button", "TextToText_Button", "SpeechToText_Button", "TextToSpeech_Button", "ObjectDetector_Button", "ImageToText_Button" })
        {
            MenuController controller = GetMenuController(workflow);
            if (controller != null && !_menuControllers.ContainsKey(workflow))
            {
                _menuControllers.Add(workflow[..^7].ToLower(), controller);
            }
            else
            {
                Debug.LogWarning($"OpenAILive: No MenuController found for workflow '{workflow}'");
            }
        }
    }

    private MenuController GetMenuController(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
        {
            return null;
        }
        MenuController controller = obj.transform.parent.gameObject.GetComponentInChildren<MenuController>();
        if (controller != null)
        {
            return controller;
        }
        return null;
    }

    public string GetSelectedMicrophoneDevice()
    {
        if (_selectedMicrophoneIndex == 0)
        {
            return null;
        }

        int deviceIndex = _selectedMicrophoneIndex - 1;

        if (Microphone.devices.Length > 0 && deviceIndex < Microphone.devices.Length)
        {
            return Microphone.devices[deviceIndex];
        }

        return null;
    }

    private string GetMicrophone()
    {
        string[] devices = Microphone.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("No microphones found.");
            return null;
        }

        string[] array = devices;
        foreach (string text in array)
        {
            if (text.ToLower().Contains("oculus"))
            {
                return text;
            }
        }

        return devices[0];
    }

    public void OnClick()
    {
        StartCoroutine(OnClickCoroutine());
    }

    public IEnumerator OnClickCoroutine()
    {
        if (_openAiRealTimeClient.IsConnected())
        {
            Task task = Disconnect();
            yield return new WaitUntil(() => task.IsCompleted);
            if (task.IsFaulted)
            {
                OnError("Disconnection failed: " + task.Exception?.GetBaseException().Message);
            }
        }
        else
        {
            Task task = Connect();
            yield return new WaitUntil(() => task.IsCompleted);
            if (task.IsFaulted)
            {
                OnError("Connection failed: " + task.Exception?.GetBaseException().Message);
            }
        }
    }

    private async Task Connect()
    {
        if (_openAiRealTimeClient.IsConnected()) return;

        UITool uiTool = new(_openAiRealTimeClient);
        List<Tool> tools = uiTool.GetTools(_menuControllers);
        Dictionary<string, Action<string, string>> functionHandlers = uiTool.GetHandlers();

        OnStatus("Connecting to OpenAI Realtime API...");
        await _openAiRealTimeClient.Connect(_secretsManager.GetSecret("OpenAI"), _systemPrompt, _voice.ToString(), tools, functionHandlers);
    }

    public async Task Disconnect()
    {
        if (!_openAiRealTimeClient.IsConnected()) return;
        if (_openAiRealTimeClient != null)
        {
            await _openAiRealTimeClient.Close();
        }
        OnStatus("Disconnected");
    }

    private void OnError(string errorMessage) {
        _statusText.text = errorMessage;
    }

    private void OnStatus(string status) => _statusText.text = status;
    private void OnTranscription(string transcription) => _agentTranscriptText.text = transcription;

    private void OnReady()
    {
    }
}
