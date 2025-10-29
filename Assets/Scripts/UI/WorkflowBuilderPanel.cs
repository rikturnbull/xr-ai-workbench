using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XrAiAccelerator;

public class WorkflowBuilderPanel : MonoBehaviour
{
    private Dictionary<string, string> _options = new();

    public void Init(PrefabManager prefabManager, string name, List<XrAiWorkflowInspectorOption> properties)
    {
        GameObject uiBackplane = prefabManager.InstantiatePrefab(PrefabManager.UI_BACKPLATE, transform);
        uiBackplane.name = $"{name}PanelBackplane";
        uiBackplane.transform.localPosition = new Vector3(0, 0, 0);

        RectTransform rectTransform = uiBackplane.transform.GetChild(0).GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, (properties.Count + 1) * 120);

        GameObject canvas = uiBackplane.transform.FindChildRecursive("UIBackplate").gameObject;

        float row = 0.0f;
        foreach (var property in properties)
        {
            _options[property.Key] = property.Value;
            GameObject uiComponent = GetUIComponentForValue(prefabManager, canvas.transform, property.Key, property.Description, property.Value, row);
            row += 50.0f;
        }

        GameObject closeButton = prefabManager.InstantiatePrefab(PrefabManager.UI_PRIMARY_BUTTON, canvas.transform);
        closeButton.transform.localPosition = new Vector3(0, 0, 0);
        closeButton.transform.FindChildRecursive("Label").GetComponent<TMPro.TextMeshProUGUI>().text = "Close";
        closeButton.transform.FindChildRecursive("Label").GetComponent<TMPro.TextMeshProUGUI>().fontSize = 20;
        closeButton.transform.FindChildRecursive("Icon").gameObject.SetActive(false);
        Button button = closeButton.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            this.gameObject.SetActive(false);
        });
    }
    
    public Dictionary<string, string> GetWorkflowOptions()
    {
        return _options;
    }

    private GameObject GetUIComponentForValue(PrefabManager prefabManager, Transform parent, string name, string description, string defaultValue, float row)
    {
        GameObject uiComponent;

        uiComponent = prefabManager.InstantiatePrefab(PrefabManager.UI_TEXT_INPUT, parent);
        uiComponent.transform.localPosition = new Vector3(0, 0, 0);
        uiComponent.transform.FindChildRecursive("Title").GetComponent<TMPro.TextMeshProUGUI>().text = name;
        uiComponent.transform.FindChildRecursive("TextField").GetComponent<TMPro.TMP_InputField>().text = defaultValue;
        uiComponent.transform.FindChildRecursive("TextField").GetComponent<TMPro.TMP_InputField>().onValueChanged.AddListener((string newValue) =>
        {
            _options[name] = newValue;
        });
        uiComponent.transform.FindChildRecursive("HelperText").GetComponent<TMPro.TextMeshProUGUI>().text = description;
        uiComponent.name = name;

        return uiComponent;
    }
}