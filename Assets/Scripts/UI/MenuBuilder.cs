using System.Collections.Generic;
using UnityEngine;
using XrAiAccelerator;
using TMPro;
using Oculus.Interaction;
using UnityEngine.UI;
using System;

public class MenuBuilder : MonoBehaviour
{
    enum WorkflowType
    {
        WORKFLOW_IMAGE_TO_3D,
        WORKFLOW_IMAGE_TO_IMAGE,
        WORKFLOW_TEXT_TO_IMAGE,
        WORKFLOW_TEXT_TO_TEXT,
        WORKFLOW_SPEECH_TO_TEXT,
        WORKFLOW_TEXT_TO_SPEECH,
        WORKFLOW_OBJECT_DETECTOR,
        WORKFLOW_IMAGE_TO_TEXT,
    }

    private const float BUTTON_SPACING = 0.03f;

    [SerializeField] private WorkflowType _workflowType;

    void Start()
    {
        List<string> providers = GetProviders(_workflowType);
        
        if (!transform.FindChildRecursive("MenuController").gameObject.TryGetComponent<MenuController>(out var menuController)) return;

        float ycoord = 0.0f;
        foreach (var provider in providers)
        {
            GameObject button = CreateButton(provider);
            button.transform.localPosition = new Vector3(0.0f, ycoord, 0.0f);
            ycoord -= BUTTON_SPACING;

            AddOnClickListener(_workflowType, button.GetComponent<InteractableUnityEventWrapper>(), provider);
            AddMenuButton(menuController, button);
        }
        menuController.Init();
    }

    private GameObject CreateButton(string providerName)
    {
        GameObject button = Instantiate(Resources.Load<GameObject>("Prefabs/Button"), transform, false);
        SetText(button, providerName);
        SetImage(button, providerName);
        return button;
    }

    private void SetText(GameObject button, string text)
    {
        GameObject textObj = button.transform.FindChildRecursive("Label").gameObject;
        if (textObj == null) return;

        textObj.GetComponent<TextMeshProUGUI>().text = ParseText(text);
    }

    // hack hack hack
    private string ParseText(string text)
    {
        if (text == "BedrockAnthropic")
        {
            return "Bedrock\nAnthropic";
        }
        else if (text == "RekognitionKeypoints")
        {
            return "Rekognition\nKeypoints";
        }
        else if (text == "RoboflowLocal")
        {
            return "Roboflow\nLocal";
        }
        return text;
    }

    private void AddMenuButton(MenuController menuController, GameObject button)
    {
        Array.Resize(ref menuController.controlledButtons, menuController.controlledButtons.Length + 1);
        menuController.controlledButtons[^1] = button;
    }

    private void SetImage(GameObject button, string providerName)
    {
        GameObject imageObj = button.transform.FindChildRecursive("Image").gameObject;
        if (imageObj == null) return;

        if (!imageObj.TryGetComponent<Image>(out var image)) return;

        Texture2D texture = Resources.Load<Texture2D>("Images/Logos/" + providerName);
        if (texture == null) return;

        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private List<string> GetProviders(WorkflowType workflowType)
    {
        switch (workflowType)
        {
            case WorkflowType.WORKFLOW_IMAGE_TO_3D:
                return XrAiFactory.GetProviderNames(typeof(IXrAiImageTo3d));
            case WorkflowType.WORKFLOW_OBJECT_DETECTOR:
                return XrAiFactory.GetProviderNames(typeof(IXrAiObjectDetector));
            case WorkflowType.WORKFLOW_IMAGE_TO_TEXT:
                return XrAiFactory.GetProviderNames(typeof(IXrAiImageToText));
            case WorkflowType.WORKFLOW_IMAGE_TO_IMAGE:
                return XrAiFactory.GetProviderNames(typeof(IXrAiImageToImage));
            case WorkflowType.WORKFLOW_TEXT_TO_IMAGE:
                return XrAiFactory.GetProviderNames(typeof(IXrAiTextToImage));
            case WorkflowType.WORKFLOW_TEXT_TO_TEXT:
                return XrAiFactory.GetProviderNames(typeof(IXrAiTextToText));
            case WorkflowType.WORKFLOW_SPEECH_TO_TEXT:
                return XrAiFactory.GetProviderNames(typeof(IXrAiSpeechToText));
            case WorkflowType.WORKFLOW_TEXT_TO_SPEECH:
                return XrAiFactory.GetProviderNames(typeof(IXrAiTextToSpeech));
            default:
                throw new ArgumentException("Unsupported workflow type");
        }
    }
    private void AddOnClickListener(WorkflowType workflowType, InteractableUnityEventWrapper button, string provider)
    {
        switch (workflowType)
        {
            case WorkflowType.WORKFLOW_IMAGE_TO_3D:
                ImageTo3dInference imageTo3d = FindAnyObjectByType<ImageTo3dInference>();
                button.WhenSelect.AddListener(() => imageTo3d.OnClick(provider));
                break;
            case WorkflowType.WORKFLOW_IMAGE_TO_TEXT:
                ImageToTextInference imageToText = FindAnyObjectByType<ImageToTextInference>();
                button.WhenSelect.AddListener(() => imageToText.OnClick(provider));
                break;
            case WorkflowType.WORKFLOW_OBJECT_DETECTOR:
                ObjectDetectorInference objectDetector = FindAnyObjectByType<ObjectDetectorInference>();
                button.WhenSelect.AddListener(() => objectDetector.OnClick(provider));
                break;
            case WorkflowType.WORKFLOW_IMAGE_TO_IMAGE:
                ImageToImageInference imageToImage = FindAnyObjectByType<ImageToImageInference>();
                button.WhenSelect.AddListener(() => imageToImage.OnClick(provider));
                break;
            case WorkflowType.WORKFLOW_TEXT_TO_IMAGE:
                TextToImageInference textToImage = FindAnyObjectByType<TextToImageInference>();
                button.WhenSelect.AddListener(() => textToImage.OnClick(provider));
                break;
            case WorkflowType.WORKFLOW_SPEECH_TO_TEXT:
                SpeechToTextInference speechToText = FindAnyObjectByType<SpeechToTextInference>();
                button.WhenSelect.AddListener(() => speechToText.OnClick(provider));
                break;
            case WorkflowType.WORKFLOW_TEXT_TO_SPEECH:
                TextToSpeechInference textToSpeech = FindAnyObjectByType<TextToSpeechInference>();
                button.WhenSelect.AddListener(() => textToSpeech.OnClick(provider));
                break;
            default:
                throw new ArgumentException("Unsupported workflow type");
        }
    }
}
