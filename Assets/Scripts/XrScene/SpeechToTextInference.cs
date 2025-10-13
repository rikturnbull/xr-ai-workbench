using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using XrAiAccelerator;
using Utilities.Async;

public class SpeechToTextInference : MonoBehaviour
{
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private TMP_Text _resultText;

    // private Task<XrAiResult<string>> _task;
    private XrAiModelManager _modelManager;
    private XrAiSpeechToTextHelper _speechToTextHelper;
    private string _model;
    private Task _currentTask;
    private CancellationTokenSource _cancellationTokenSource;

    void Start()
    {
        _speechToTextHelper = gameObject.AddComponent<XrAiSpeechToTextHelper>();
        _modelManager = XrAiModelManager.GetModelManager();
    }

    public void OnClick(string model)
    {
        try
        {
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
        StartCoroutine(ExecuteCoroutine(audioData, OnSpeechToTextResult));
    }

    private IEnumerator ExecuteCoroutine(byte[] audioData, Action<XrAiResult<string>> callback)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        Dictionary<string, string> globalProperties = _modelManager.GetGlobalProperties(_model);
        IXrAiSpeechToText speechToText = XrAiFactory.LoadSpeechToText(_model);
        speechToText.Initialize(globalProperties);
        yield return null;

        if (_cancellationTokenSource.Token.IsCancellationRequested) yield break;

        _currentTask = speechToText.Execute(
            audioData,
            _modelManager.GetWorkflowProperties(
                _model,
                XrAiFactory.WORKFLOW_SPEECH_TO_TEXT
            ),
            callback
        ).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    private void OnSpeechToTextResult(XrAiResult<string> result)
    {
        if (!result.IsSuccess)
        {
            Debug.LogException(new Exception(result.ErrorMessage));
            _statusText.text = result.ErrorMessage;
            return;
        }

        _statusText.text = "Transcription complete.";
        _resultText.text = result.Data ?? "No text recognized.";
    }
}
