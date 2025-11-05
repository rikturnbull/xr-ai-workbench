using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    public static readonly string UI_BACKPLATE = "UIBackplate";
    public static readonly string UI_DROPDOWN = "UIDropdown";
    public static readonly string UI_PRIMARY_BUTTON = "UIPrimaryButton";
    public static readonly string UI_SECONDARY_BUTTON = "UISecondaryButton";
    public static readonly string UI_SLIDER = "UISlider";
    public static readonly string UI_TEXT_INPUT = "UITextInput";

    private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();

    public GameObject LoadPrefab(string prefabName)
    {
        if (prefabCache.TryGetValue(prefabName, out GameObject cachedPrefab))
        {
            return cachedPrefab;
        }
        GameObject prefab = Resources.Load<GameObject>("Prefabs/" + prefabName);
        if (prefab == null)
        {
            Debug.LogError($"Prefab '{prefabName}' not found in Resources.");
            return null;
        }
        prefabCache[prefabName] = prefab;
        return prefab;
    }

    public GameObject InstantiatePrefab(string prefabName, Transform parent)
    {
        GameObject prefab = LoadPrefab(prefabName);
        if (prefab != null)
        {
            return Instantiate(prefab, parent);
        }
        return null;
    }
}