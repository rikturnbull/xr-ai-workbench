using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Async;
using XrAiAccelerator;

public class TextToImageInference : BaseAiInference<IXrAiTextToImage, Texture2D>
{
    protected override IXrAiTextToImage LoadProvider(string provider)
    {
        return XrAiFactory.LoadTextToImage(provider);
    }

    protected override IEnumerator InitializeProvider(IXrAiTextToImage loadedProvider, Dictionary<string, string> options)
    {
        _currentTask = loadedProvider.Initialize(options).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    protected override IEnumerator ExecuteProvider(IXrAiTextToImage loadedProvider, string provider, Dictionary<string, string> options, Action<XrAiResult<Texture2D>> callback)
    {
        if (_cancellationTokenSource.Token.IsCancellationRequested) yield break;

        options["prompt"] = GetPromptText();
        
        _currentTask = loadedProvider.Execute(
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
