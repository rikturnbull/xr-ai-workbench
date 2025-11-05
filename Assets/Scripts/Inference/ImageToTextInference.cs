using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XrAiAccelerator;
using Utilities.Async;

public class ImageToTextInference : BaseAiInference<IXrAiImageToText, string>
{
    protected override IXrAiImageToText LoadProvider(string provider)
    {
        return XrAiFactory.LoadImageToText(provider);
    }

    protected override IEnumerator InitializeProvider(IXrAiImageToText loadedProvider, Dictionary<string, string> options)
    {
        _currentTask = loadedProvider.Initialize(options).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    protected override IEnumerator ExecuteProvider(IXrAiImageToText loadedProvider, string provider, Dictionary<string, string> options, Action<XrAiResult<string>> callback)
    {
        if (_cancellationTokenSource.Token.IsCancellationRequested) yield break;

        Texture2D texture = GetTexture();
        if (texture == null)
        {
            callback(XrAiResult.Failure<string>("No texture available for inference."));
            yield break;
        }

        _currentTask = loadedProvider.Execute(
            texture,
            options,
            callback
        ).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
}

    protected override void OnInferenceResult(XrAiResult<string> result)
    {
        SetResultText(result.Data);
    }
}
