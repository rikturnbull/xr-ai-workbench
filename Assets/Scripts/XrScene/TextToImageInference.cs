using System;
using System.Collections.Generic;
using UnityEngine;
using XrAiAccelerator;

public class TextToImageInference : BaseAiInference<Texture2D>
{
    protected override void Execute(string model, Dictionary<string, string> globalProperties)
    {
        IXrAiTextToImage textToImage = XrAiFactory.LoadTextToImage(model, globalProperties);
        Dictionary<string, string> properties = GetWorkflowProperties(model, XrAiModelManager.WORKFLOW_TEXT_TO_IMAGE);
        properties["prompt"] = _resultText.text;
        _task = textToImage.Execute(properties);
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
