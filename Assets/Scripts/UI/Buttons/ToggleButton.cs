using UnityEngine;
using Oculus.Interaction;
using System.Collections.Generic;
using System;

public class ToggleButton : MonoBehaviour
{
    [Header("Toggle Settings")]
    public bool isToggled = false;
    public List<ToggleButton> peers;
    
    [Header("Visual Feedback")]
    [Tooltip("Drag the ButtonPanel GameObject here")]
    public GameObject buttonPanel;
    
    [Header("Color Settings")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.gray;
    public Color selectedColor = Color.cyan;
    
    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public AudioClip toggleOnSound;
    public AudioClip toggleOffSound;
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnToggleOn;
    public UnityEngine.Events.UnityEvent OnToggleOff;
    
    public bool _ignoreButtonPress = false;

    private InteractableUnityEventWrapper eventWrapper;
    private InteractableColorVisual colorVisual;
    private MeshRenderer buttonRenderer;
    private Material buttonMaterial;
    
    void Start()
    {
        // Get the event wrapper from this button
        eventWrapper = GetComponent<InteractableUnityEventWrapper>();
        
        // Get the InteractableColorVisual from the ButtonPanel
        if (buttonPanel != null)
        {
            colorVisual = buttonPanel.GetComponent<InteractableColorVisual>();
            buttonRenderer = buttonPanel.GetComponent<MeshRenderer>();
        }
        
        // If buttonPanel not assigned, try to find it automatically
        if (buttonPanel == null)
        {
            // Look for ButtonPanel in children
            Transform buttonPanelTransform = transform.Find("Visuals/ButtonVisual/ButtonPanel");
            if (buttonPanelTransform != null)
            {
                buttonPanel = buttonPanelTransform.gameObject;
                colorVisual = buttonPanel.GetComponent<InteractableColorVisual>();
                buttonRenderer = buttonPanel.GetComponent<MeshRenderer>();
                Debug.Log($"Found ButtonPanel automatically: {buttonPanel.name}");
            }
            else
            {
                Debug.LogError("ButtonPanel not found! Please assign it manually in the ToggleButton component.");
            }
        }
        
        // Create material instance to avoid affecting other buttons
        if (buttonRenderer != null)
        {
            buttonMaterial = new Material(buttonRenderer.material);
            buttonRenderer.material = buttonMaterial;
            Debug.Log($"Created material instance for {buttonPanel.name}");
        }

        // Connect to the interaction events
        if (eventWrapper != null && !_ignoreButtonPress)
        {
            eventWrapper.WhenSelect.AddListener(OnButtonPressed);
        }
        else
        {
            Debug.LogError("InteractableUnityEventWrapper not found on the ToggleButton!");
        }
        
        // Set initial visual state
        UpdateVisualState();
    }

    public void OnButtonPressed()
    {
        Debug.Log($"{DateTime.Now:HH:mm:ss.fff}Button pressed: {(isToggled ? "ON" : "OFF")} - Color: {(isToggled ? selectedColor : normalColor)}");
        // Toggle the state
        isToggled = !isToggled;

        // Update visual state
        UpdateVisualState();

        if(isToggled)
        {
            UntogglePeers();
        }

        // Play appropriate sound
        if (audioSource != null)
        {
            if (isToggled && toggleOnSound != null)
                audioSource.PlayOneShot(toggleOnSound);
            else if (!isToggled && toggleOffSound != null)
                audioSource.PlayOneShot(toggleOffSound);
        }

        // Trigger appropriate events
        TriggerToggleEvents();

        Debug.Log($"{DateTime.Now:HH:mm:ss.fff}Button toggled: {(isToggled ? "ON" : "OFF")} - Color: {(isToggled ? selectedColor : normalColor)}");
    }
    
    private void UntogglePeers()
    {
        if (peers == null) return;
        foreach (ToggleButton peer in peers)
        {
            if (peer.isToggled)
            {
                peer.SetToggle(false);
            }
        }
    }
    private void TriggerToggleEvents()
    {
        // Trigger appropriate events based on current state
        if (isToggled)
            OnToggleOn.Invoke();
        else
            OnToggleOff.Invoke();
    }
    
    private void UpdateVisualState()
    {
        if (buttonMaterial == null) return;
        
        if (isToggled)
        {
            // Disable InteractableColorVisual when toggled on
            if (colorVisual != null)
                colorVisual.enabled = false;
                
            // Set to selected color when toggled on
            buttonMaterial.color = selectedColor;
            Debug.Log($"Setting color to select: {selectedColor}");
        }
        else
        {
            // Re-enable InteractableColorVisual when toggled off
            if (colorVisual != null)
                colorVisual.enabled = true;
                
            // Set to normal color when toggled off
            buttonMaterial.color = normalColor;
            Debug.Log($"Setting color to normal: {normalColor}");
        }
    }
    
    // Override the normal hover behavior when toggled
    void Update()
    {
        if (isToggled && buttonMaterial != null)
        {
            // Keep the selected color even during hover when toggled on
            buttonMaterial.color = selectedColor;
        }
    }
    
    // FIXED: Public methods to control toggle state - NOW TRIGGERS EVENTS!
    public void SetToggle(bool state)
    {
        bool wasToggled = isToggled;
        isToggled = state;
        UpdateVisualState();
        
        // Only trigger events if the state actually changed
        if (wasToggled != isToggled)
        {
            TriggerToggleEvents();
        }
    }
    
    public void SetToggleWithoutEvents(bool state)
    {
        // For cases where you want to change state without triggering events
        isToggled = state;
        UpdateVisualState();
    }
    
    public void ToggleState()
    {
        OnButtonPressed();
    }
    
    // Method to disable/enable the InteractableColorVisual when toggled
    public void SetColorVisualEnabled(bool enabled)
    {
        if (colorVisual != null)
        {
            colorVisual.enabled = enabled;
        }
    }
}
