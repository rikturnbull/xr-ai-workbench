using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.Async;
using XrAiAccelerator;

public class SpeechToTextInference : BaseAiInference<IXrAiSpeechToText, string>
{
    private XrAiSpeechToTextHelper _speechToTextHelper;

    protected override void SetUp()
    {
        if (_speechToTextHelper == null)
        {
            _speechToTextHelper = gameObject.AddComponent<XrAiSpeechToTextHelper>();
        }
    }

    protected override IXrAiSpeechToText LoadProvider(string provider)
    {
        return XrAiFactory.LoadSpeechToText(provider);
    }

    protected override IEnumerator InitializeProvider(IXrAiSpeechToText loadedProvider, Dictionary<string, string> options)
    {
        _currentTask = loadedProvider.Initialize(options).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    protected override IEnumerator ExecuteProvider(IXrAiSpeechToText loadedProvider, string provider, Dictionary<string, string> options, Action<XrAiResult<string>> callback)
    {
        yield return null;
                
        if (_cancellationTokenSource.Token.IsCancellationRequested) yield break;

        SetStatusText("Recording audio for 5 seconds...");

        options.TryGetValue("mediaEncoding", out string mediaEncoding);
        options.TryGetValue("sampleRate", out string sampleRateStr);
        int? sampleRate = sampleRateStr != null && int.TryParse(sampleRateStr, out int rate) ? rate : null;

        _speechToTextHelper.StartRecording((audioData) =>
        {
            StopTimer();
            SetStatusText("Processing recorded audio...");
            StartTimer();

            _currentTask = loadedProvider.Execute(
                audioData,
                options,
                callback
            ).WithCancellation(_cancellationTokenSource.Token);
        }, null, mediaEncoding, 5, sampleRate);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    protected override void OnInferenceResult(XrAiResult<string> result)
    {
        SetResultText(result.Data);
    }
}
