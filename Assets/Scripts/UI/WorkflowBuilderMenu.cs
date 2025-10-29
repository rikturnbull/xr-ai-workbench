using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkflowBuilderMenu : MonoBehaviour
{
    GameObject _uiBackplaneCanvas;

    public void Init(PrefabManager prefabManager, int nTypes, int maxImplementations)
    {
        GameObject uiBackplane = prefabManager.InstantiatePrefab(PrefabManager.UI_BACKPLATE, transform);
        uiBackplane.name = "WorkflowBuilderMenuBackplane";
        uiBackplane.transform.localPosition = Vector3.zero;

        RectTransform rectTransform = uiBackplane.transform.GetChild(0).GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(nTypes * 120, maxImplementations * 50);

        GameObject canvasRoot = uiBackplane.transform.FindChildRecursive("CanvasRoot").gameObject;
        canvasRoot.transform.localPosition = Vector3.zero;
        canvasRoot.transform.localRotation = Quaternion.identity;
        canvasRoot.transform.localScale = Vector3.one;

        _uiBackplaneCanvas = uiBackplane.transform.FindChildRecursive("UIBackplate").gameObject;
        _uiBackplaneCanvas.transform.localPosition = Vector3.zero;
        _uiBackplaneCanvas.transform.localRotation = Quaternion.identity;
        _uiBackplaneCanvas.transform.localScale = Vector3.one;
        if (_uiBackplaneCanvas.TryGetComponent<VerticalLayoutGroup>(out var layoutGroup))
        {
            Destroy(layoutGroup);
        }
        DestroyImmediate(layoutGroup);

        uiBackplane.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
    }

    public void AddWorkflow(PrefabManager prefabManager, WorkflowBuilder workflowBuilder, string workflowName, Dictionary<string, WorkflowBuilderPanel> implementationPanels)
    {

        GameObject canvas = new GameObject("Canvas");
        canvas.transform.parent = _uiBackplaneCanvas.transform;
        canvas.transform.localPosition = Vector3.zero;
        canvas.transform.localRotation = Quaternion.identity;
        canvas.transform.localScale = Vector3.one;

        canvas.AddComponent<Canvas>();
        VerticalLayoutGroup vertical = canvas.AddComponent<VerticalLayoutGroup>();
        vertical.childForceExpandHeight = false;
        vertical.childForceExpandWidth = false;
        vertical.childAlignment = TextAnchor.UpperCenter;
        vertical.padding = new RectOffset(5, 5, 10, 0);
        vertical.spacing = 5f;

        GameObject heading = new GameObject("Heading");
        heading.transform.parent = canvas.transform;
        heading.transform.localPosition = Vector3.zero;
        heading.transform.localRotation = Quaternion.identity;
        heading.transform.localScale = Vector3.one;
        TMPro.TextMeshProUGUI title = heading.AddComponent<TMPro.TextMeshProUGUI>();
        title.text = workflowName.Substring(5);
        title.fontSize = 12;

        foreach (var impl in implementationPanels.Keys)
        {
            GameObject buttonObj = prefabManager.InstantiatePrefab(PrefabManager.UI_PRIMARY_BUTTON, canvas.transform);
            buttonObj.transform.localPosition = Vector3.zero;
            buttonObj.transform.localRotation = Quaternion.identity;

            Button button = buttonObj.GetComponent<Button>();
            button.transform.FindChildRecursive("Label").GetComponent<TMPro.TextMeshProUGUI>().text = ParseText(impl);

            Texture2D texture = Resources.Load<Texture2D>("Images/Logos/" + impl);
            if (texture == null) return;

            GameObject iconObj = buttonObj.transform.FindChildRecursive("Icon").gameObject;
            Image image = iconObj.GetComponent<Image>();
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            button.onClick.AddListener(() =>
            {
                GameObject panel = implementationPanels[impl].gameObject;
                workflowBuilder.CloseAllPanels();
                panel.SetActive(!panel.activeSelf);
            });
        }
    }
    
    // hack hack hack
    private string ParseText(string text)
    {
        if (text == "BedrockAnthropic")
        {
            return "Bedrock\nAnthropic";
        }
        else if (text == "BedrockTitan")
        {
            return "Bedrock\nTitan";
        }
        else if (text == "RekognitionKeypoints")
        {
            return "Rekognition\nKeypoints";
        }
        else if(text == "RoboflowLocal")
        {
            return "Roboflow\nLocal";
        }
        return text;
    }
}