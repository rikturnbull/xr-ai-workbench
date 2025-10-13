using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using XrAiAccelerator;
using Utilities.Async;

public class ObjectDetectorInference : BaseAiInference<XrAiBoundingBox[]>
{
    [SerializeField] private YoloAssets _yoloModelAsset;

    protected override void Execute(string model, Dictionary<string, string> globalProperties)
    {
        StartCoroutine(ExecuteCoroutine(model, globalProperties));
    }

    private IEnumerator ExecuteCoroutine(string model, Dictionary<string, string> globalProperties)
    {
        Debug.Log($"Loading object detection model: {model}");
        IXrAiObjectDetector objectDetector = XrAiFactory.LoadObjectDetector(model);
        if (objectDetector == null)
        {
            Debug.LogError($"Failed to load object detector model: {model}");
            yield break;
        }
        _currentTask = objectDetector.Initialize(globalProperties, _yoloModelAsset).WithCancellation(_cancellationTokenSource.Token);
        Debug.Log("Object detector model initialized.");
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
        if(_cancellationTokenSource.Token.IsCancellationRequested) Debug.Log("Object detection cancelled after initialization.");
        if (_cancellationTokenSource.Token.IsCancellationRequested) yield break;

        Debug.Log("Starting object detection...");
        _currentTask = objectDetector.Execute(
            GetFrame().GetTexture(),
            GetWorkflowProperties(model, XrAiFactory.WORKFLOW_OBJECT_DETECTOR),
            OnObjectsDetected
        ).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    private void OnObjectsDetected(XrAiResult<XrAiBoundingBox[]> result)
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

    protected override void ProcessResult(XrAiBoundingBox[] data)
    {
        if (data == null || data.Length == 0)
        {
            throw new ArgumentException("Received empty data for object detection.");
        }
        _statusText.text = $"Detected {data.Length} objects";
        FrameBehaviour frame = _pad.GetFrame();
        if (frame == null)
        {
            throw new Exception("Input image frame is not set.");
        }

        frame.DrawBoundingBoxes(data);
    }
}

