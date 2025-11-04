using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Async;
using XrAiAccelerator;

public class ObjectDetectorInference : BaseAiInference<IXrAiObjectDetector, XrAiBoundingBox[]>
{
    protected override IXrAiObjectDetector LoadProvider(string provider)
    {
        return XrAiFactory.LoadObjectDetector(provider);
    }

    protected override IEnumerator InitializeProvider(IXrAiObjectDetector loadedProvider, Dictionary<string, string> options)
    {
        _currentTask = loadedProvider.Initialize(options).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    protected override IEnumerator ExecuteProvider(IXrAiObjectDetector loadedProvider, string provider, Dictionary<string, string> options, Action<XrAiResult<XrAiBoundingBox[]>> callback)
    {
        if (_cancellationTokenSource.Token.IsCancellationRequested) yield break;

        if (GetFrame().GetIsVideo())
        {
            float startTime = Time.time;
            while (Time.time - startTime < 30f)
            {
                StartTimer();
                Texture2D texture = GetTexture();
                if (texture == null)
                {
                    callback(XrAiResult.Failure<XrAiBoundingBox[]>("No texture available for inference."));
                    yield break;
                }

                _currentTask = loadedProvider.Execute(
                    texture,
                    options,
                    callback
                ).WithCancellation(_cancellationTokenSource.Token);
                yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
                StopTimer();
            }
        }
        else
        {
            Texture2D texture = GetTexture();
            if (texture == null)
            {
                callback(XrAiResult.Failure<XrAiBoundingBox[]>("No texture available for inference."));
                yield break;
            }

            _currentTask = loadedProvider.Execute(
                texture,
                GetWorkflowOptions(provider),
                callback
            ).WithCancellation(_cancellationTokenSource.Token);
            yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
        }
    }

    protected override void OnInferenceResult(XrAiResult<XrAiBoundingBox[]> result)
    {
        FrameBehaviour frame = GetFrame();
        if (frame == null)
        {
            SetStatusText("Input image frame is not set.");
            return;
        }
        frame.ClearBoundingBoxes();
     
        if (result.Data.Length == 0)
        {
            SetStatusText("Received empty data for object detection.");
            return;
        }

        SetStatusText($"Detected {result.Data.Length} objects");
        frame.DrawBoundingBoxes(result.Data);
    }
}

