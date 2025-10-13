using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.Async;
using XrAiAccelerator;

public class TextToImageInference : BaseAiInference<Texture2D>
{
    protected override void Execute(string model, Dictionary<string, string> globalProperties)
    {

        StartCoroutine(ExecuteCoroutine(model, globalProperties));
    }

    private IEnumerator ExecuteCoroutine(string model, Dictionary<string, string> globalProperties)
    {
        IXrAiTextToImage textToImage = null;
        try
        {
            textToImage = XrAiFactory.LoadTextToImage(model);
            textToImage.Initialize(globalProperties);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            _statusText.text = ex.Message;
        }
        if(textToImage == null) yield break;
        yield return null;


        if (_cancellationTokenSource.Token.IsCancellationRequested) yield break;

        Dictionary<string, string> properties = GetWorkflowProperties(model, XrAiFactory.WORKFLOW_TEXT_TO_IMAGE);
        properties["prompt"] = _resultText.text;

        _currentTask = textToImage.Execute(
            properties,
            OnTextToImageResult
        ).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    private void OnTextToImageResult(XrAiResult<Texture2D> result)
    {
        StopTimer();
        if (result.IsSuccess)
        {
            ProcessResult(result.Data);
        }
        else
        {
            Debug.LogError($"Text to Image generation failed: {result.ErrorMessage}");
        }
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
