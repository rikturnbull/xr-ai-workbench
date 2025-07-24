using System;
using System.Collections.Generic;
using XrAiAccelerator;

public class ImageToTextInference : BaseAiInference<string>
{
    protected override void Execute(string model, Dictionary<string, string> globalProperties)
    {
        string imageFormat = "image/jpeg";
        byte[] imageBytes = XrAiImageHelper.EncodeTexture(GetFrame().GetTexture(), imageFormat);
        IXrAiImageToText imageToText = XrAiFactory.LoadImageToText(model, globalProperties);
        _task = imageToText.Execute(imageBytes, imageFormat, GetWorkflowProperties(model, XrAiModelManager.WORKFLOW_IMAGE_TO_TEXT));
    }

    protected override void ProcessResult(string data)
    {
        _resultText.text = data ?? throw new ArgumentException("Received empty data.");
    }
}
