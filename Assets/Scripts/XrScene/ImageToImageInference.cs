using System;
using System.Collections.Generic;
using UnityEngine;
using XrAiAccelerator;

public class ImageToImageInference : BaseAiInference<Texture2D>
{
    protected override void Execute(string model, Dictionary<string, string> globalProperties)
    {
        IXrAiImageToImage imageToImage = XrAiFactory.LoadImageToImage(model, globalProperties);
        _task = imageToImage.Execute(GetFrame().GetTexture(), GetWorkflowProperties(model, XrAiModelManager.WORKFLOW_IMAGE_TO_IMAGE));
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
