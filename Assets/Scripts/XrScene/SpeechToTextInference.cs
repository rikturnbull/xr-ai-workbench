using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using XrAiAccelerator;

public class SpeechToTextInference : MonoBehaviour
{
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private TMP_Text _resultText;
    [SerializeField] private ParticleSystem _loadingParticles;

    private Task<XrAiResult<string>> _task;
    private XrAiModelManager _modelManager;
    private XrAiSpeechToTextHelper _speechToTextHelper;
    private string _model;

    void Start()
    {
        _speechToTextHelper = gameObject.AddComponent<XrAiSpeechToTextHelper>();
        _modelManager = FindFirstObjectByType<XrAiModelManager>();
    }

    private void Update()
    {
        if (_task != null)
        {
            if (!_task.IsCompleted) return;

            if (_task.IsFaulted)
            {
                _statusText.text = _task.Exception.Message;
                _loadingParticles.Stop();
                _task = null;
                return;
            }

            XrAiResult<string> result = _task.Result;
            if (result == null)
            {
                _statusText.text = "No result returned from speech to text.";
                _loadingParticles.Stop();
                _task = null;
                return;
            }

            _resultText.text = result.Data;
            _loadingParticles.Stop();
            _task = null;
        }
    }

    public void OnClick(string model)
    {
        try
        {
            _loadingParticles.Play();
            _statusText.text = "Recording for 5 seconds...";
            _model = model;
            _speechToTextHelper.StartRecording(
                GetMicrophone(),
                OnRecordingComplete,
                5
            );
        }
        catch (Exception ex)
        {
            _loadingParticles.Stop();
            _statusText.text = ex.Message;
        }
    }

    private string GetMicrophone()
    {
        var availableMicrophones = Microphone.devices;
        if (availableMicrophones.Length == 0)
        {
            Debug.LogError("No microphones found.");
            return null;
        }
        return availableMicrophones[0];
    }

    private void OnRecordingComplete(byte[] audioData)
    {
        _statusText.text = "Recording complete, processing audio...";
        try
        {
            Dictionary<string, string> globalProperties = _modelManager.GetGlobalProperties(_model);
            IXrAiSpeechToText speechToText = XrAiFactory.LoadSpeechToText(_model, globalProperties);
            Dictionary<string, string> properties = _modelManager.GetWorkflowProperties(
                _model,
                XrAiModelManager.WORKFLOW_SPEECH_TO_TEXT
            );
            _task = speechToText.Execute(audioData, properties);
        }
        catch (Exception ex)
        {
            _loadingParticles.Stop();
            _statusText.text = ex.Message;
            return;
        }
    }
}
