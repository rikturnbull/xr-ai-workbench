using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using XrAiAccelerator;
using Utilities.Async;

public class ImageToImageInference : BaseAiInference<Texture2D>
{
    protected override void Execute(string model, Dictionary<string, string> globalProperties)
    {
        StartCoroutine(ExecuteCoroutine(model, globalProperties));
    }

    private IEnumerator ExecuteCoroutine(string model, Dictionary<string, string> globalProperties)
    {
        IXrAiImageToImage imageToImage = XrAiFactory.LoadImageToImage(model);
        imageToImage.Initialize(globalProperties);
        yield return null;

        if (_cancellationTokenSource.Token.IsCancellationRequested) yield break;

        _currentTask = imageToImage.Execute(
            GetFrame().GetTexture(),
            GetWorkflowProperties(model, XrAiFactory.WORKFLOW_IMAGE_TO_IMAGE),
            OnImageToImageResult
        ).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    private void OnImageToImageResult(XrAiResult<Texture2D> result)
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

    protected override void ProcessResult(Texture2D data)
    {
        if (data == null)
        {
            throw new ArgumentException("Received empty data.");
        }

        CreateFrameBehaviourFromPrefab(data);
    }
}
