using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Async;
using XrAiAccelerator;

public class TextToSpeechInference : BaseAiInference<IXrAiTextToSpeech, AudioClip>
{
    private AudioSource _audioSource;

    protected override IXrAiTextToSpeech LoadProvider(string provider)
    {
        return XrAiFactory.LoadTextToSpeech(provider);
    }

    protected override IEnumerator InitializeProvider(IXrAiTextToSpeech loadedProvider, Dictionary<string, string> options)
    {
        _currentTask = loadedProvider.Initialize(options).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    protected override IEnumerator ExecuteProvider(IXrAiTextToSpeech loadedProvider, string provider, Dictionary<string, string> options, Action<XrAiResult<AudioClip>> callback)
    {
        if (_cancellationTokenSource.Token.IsCancellationRequested) yield break;

        _currentTask = loadedProvider.Execute(
            GetPromptText(),
            options,
            callback
        ).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    protected override void OnInferenceResult(XrAiResult<AudioClip> result)
    {
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }

        if (_audioSource != null)
        {
            _audioSource.PlayOneShot(result.Data);
            SetStatusText("Playing audio...");
        }
        else
        {
            SetStatusText("Audio generated successfully (no AudioSource found).");
        }
    }
}
