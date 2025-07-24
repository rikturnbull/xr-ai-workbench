using System;
using System.Collections.Generic;
using UnityEngine;
using XrAiAccelerator;

public class ObjectDetectorInference : BaseAiInference<XrAiBoundingBox[]>
{
    [SerializeField] private YoloAssets _yoloModelAsset;

    protected override void Execute(string model, Dictionary<string, string> globalProperties)
    {
        IXrAiObjectDetector objectDetector = XrAiFactory.LoadObjectDetector(model, globalProperties, _yoloModelAsset);
        _task = objectDetector.Execute(GetFrame().GetTexture(), GetWorkflowProperties(model, XrAiModelManager.WORKFLOW_OBJECT_DETECTOR));
    }

    protected override void ProcessResult(XrAiBoundingBox[] data)
    {
        if(data == null || data.Length == 0)
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

