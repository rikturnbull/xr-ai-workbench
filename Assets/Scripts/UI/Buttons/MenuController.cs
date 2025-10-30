using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;

public class MenuController : MonoBehaviour
{
    [Header("Menu Control")]
    public ToggleButton menuButton;
    public GameObject[] controlledButtons;

    [Header("Animation Settings")]
    public bool useAnimation = true;
    public float animationDuration = 0.3f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();

    public void Init()
    {
        // Store original scales for each button
        foreach (GameObject button in controlledButtons)
        {
            if (button != null)
            {
                originalScales[button] = button.transform.localScale;
            }
        }

        if (menuButton != null)
        {
            menuButton.OnToggleOn.AddListener(() => SetButtonsActive(true));
            menuButton.OnToggleOff.AddListener(() => SetButtonsActive(false));
        }

        // Start with buttons hidden
        SetButtonsActive(false);
    }

    public void ToggleMenu()
    {
        if (menuButton != null)
        {
            menuButton.ToggleState();
        }
    }

    public bool IsMenuOpen()
    {
        return menuButton != null && menuButton.isToggled;
    }

    public void ClickProvider(string providerName)
    {
        Debug.Log($"MenuController: Provider '{providerName}' clicked.");
        foreach (var button in controlledButtons)
        {
            if (button != null && button.name.Equals(providerName, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"MenuController: Found button for provider '{providerName}'. Invoking click.");
                if (button.TryGetComponent<InteractableUnityEventWrapper>(out var eventWrapper))
                {
                    eventWrapper.WhenSelect.Invoke();
                }
                return;
            }
        }
        // Implement provider click handling logic here
    }

    private void SetButtonsActive(bool active)
    {
        foreach (GameObject button in controlledButtons)
        {
            if (button != null)
            {
                if (useAnimation)
                {
                    StartCoroutine(AnimateButton(button, active));
                }
                else
                {
                    button.SetActive(active);
                }
            }
        }
    }

    private IEnumerator AnimateButton(GameObject button, bool show)
    {
        Vector3 originalScale = originalScales.ContainsKey(button) ? originalScales[button] : Vector3.one;

        if (show)
        {
            // Activate button and start from zero scale
            button.SetActive(true);
            button.transform.localScale = Vector3.zero;

            // Animate to original scale
            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / animationDuration;
                float curveValue = animationCurve.Evaluate(progress);

                button.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, curveValue);
                yield return null;
            }

            // Ensure final scale is correct
            button.transform.localScale = originalScale;
        }
        else
        {
            // Animate from original scale to zero
            // Vector3 startScale = button.transform.localScale;
            Vector3 startScale = originalScales.ContainsKey(button) ? originalScales[button] : Vector3.one;
            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / animationDuration;
                float curveValue = animationCurve.Evaluate(progress);

                button.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, curveValue);
                yield return null;
            }

            // Set final scale and deactivate
            button.transform.localScale = Vector3.zero;
            button.SetActive(false);
        }
    }
}
