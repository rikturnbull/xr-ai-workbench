using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.Async;
using XrAiAccelerator;

public class ImageToTextInference : BaseAiInference<string>
{
    protected override void Execute(string model, Dictionary<string, string> globalProperties)
    {
        StartCoroutine(ExecuteCoroutine(model, globalProperties));
    }

    private IEnumerator ExecuteCoroutine(string provider, Dictionary<string, string> globalProperties)
    {
        IXrAiImageToText imageToText;
        try {
            imageToText = XrAiFactory.LoadImageToText(provider);
            imageToText.Initialize(globalProperties);
        } catch (Exception e) {
            Debug.LogError($"Failed to load ImageToText provider: {e.Message}");
            StopTimer();
            yield break;
        }
        yield return null;

        if (_cancellationTokenSource.Token.IsCancellationRequested) yield break;

        _currentTask = imageToText.Execute(
            GetFrame().GetTexture(),
            GetWorkflowProperties(provider, XrAiFactory.WORKFLOW_IMAGE_TO_TEXT),
            OnImageToTextResult
        ).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    private void OnImageToTextResult(XrAiResult<string> result)
    {
        StopTimer();

        if (!result.IsSuccess)
        {
            Debug.LogException(new Exception(result.ErrorMessage));
            _statusText.text = result.ErrorMessage;
            return;
        }

        ProcessResult(result.Data);
    }

    protected override void ProcessResult(string data)
    {
        _resultText.text = data ?? throw new ArgumentException("Received empty data.");
    }
}
