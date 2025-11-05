using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Async;
using XrAiAccelerator;

public class ImageToImageInference : BaseAiInference<IXrAiImageToImage, Texture2D>
{
    protected override IXrAiImageToImage LoadProvider(string provider)
    {
        return XrAiFactory.LoadImageToImage(provider);
    }

    protected override IEnumerator InitializeProvider(IXrAiImageToImage loadedProvider, Dictionary<string, string> options)
    {
        _currentTask = loadedProvider.Initialize(options).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    protected override IEnumerator ExecuteProvider(IXrAiImageToImage loadedProvider, string provider, Dictionary<string, string> options, Action<XrAiResult<Texture2D>> callback)
    {
        if (_cancellationTokenSource.Token.IsCancellationRequested) yield break;

        Texture2D texture = GetTexture();
        if (texture == null)
        {
            callback(XrAiResult.Failure<Texture2D>("No texture available for inference."));
            yield break;
        }

        _currentTask = loadedProvider.Execute(
            texture,
            options,
            callback
        ).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    protected override void OnInferenceResult(XrAiResult<Texture2D> result)
    {
        CreateFrameBehaviourFromPrefab(result.Data);
    }
}
