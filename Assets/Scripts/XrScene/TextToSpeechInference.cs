using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.Async;
using XrAiAccelerator;

public class TextToSpeechInference : BaseAiInference<AudioClip>
{
    [SerializeField] private AudioSource _audioSource;

    protected override void Execute(string model, Dictionary<string, string> globalProperties)
    {
        if (string.IsNullOrEmpty(_resultText.text))
        {
            throw new ArgumentException("No text to convert to speech.");
        }

        StartCoroutine(ExecuteCoroutine(model, globalProperties));
    }

    private IEnumerator ExecuteCoroutine(string model, Dictionary<string, string> globalProperties)
    {
        IXrAiTextToSpeech textToSpeech = XrAiFactory.LoadTextToSpeech(model);
        textToSpeech.Initialize(globalProperties);
        yield return null;

        if (_cancellationTokenSource.Token.IsCancellationRequested) yield break;

        _currentTask = textToSpeech.Execute(
            _resultText.text,
            GetWorkflowProperties(model, XrAiFactory.WORKFLOW_TEXT_TO_SPEECH),
            OnTextToSpeechResult
        ).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    private void OnTextToSpeechResult(XrAiResult<AudioClip> result)
    {
        if (result.IsSuccess)
        {
            ProcessResult(result.Data);
        }
        else
        {
            Debug.LogError($"Text-to-speech failed: {result.ErrorMessage}");
        }
    }

    protected override void ProcessResult(AudioClip data)
    {
        if (data == null)
        {
            throw new ArgumentException("Received empty audio data.");
        }

        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }

        if (_audioSource != null)
        {
            _audioSource.PlayOneShot(data);
            _statusText.text = "Playing audio...";
        }
        else
        {
            _statusText.text = "Audio generated successfully (no AudioSource found).";
        }
    }
}
