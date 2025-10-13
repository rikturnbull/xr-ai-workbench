using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OneButtonOnOnly : MonoBehaviour
{
    [Header("Button Group Settings")]
    [Tooltip("All buttons that should be part of this exclusive group")]
    public List<ToggleButton> toggleButtons = new List<ToggleButton>();
    
    [Header("Group Behavior")]
    [Tooltip("Allow all buttons to be off, or must one always be selected?")]
    public bool allowAllOff = true;
    
    [Tooltip("Which button should be selected by default (leave empty for none)")]
    public ToggleButton defaultSelectedButton;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private ToggleButton currentlySelectedButton;
    private bool isHandlingToggle = false; // Prevent infinite loops
    
    void Start()
    {
        // Find all ToggleButton components if list is empty
        if (toggleButtons.Count == 0)
        {
            FindToggleButtonsAutomatically();
        }
        
        // Subscribe to each button's toggle events
        foreach (ToggleButton button in toggleButtons)
        {
            if (button != null)
            {
                // Listen to BOTH on and off events
                button.OnToggleOn.AddListener(() => OnButtonToggledOn(button));
                button.OnToggleOff.AddListener(() => OnButtonToggledOff(button));
                
                if (showDebugLogs) Debug.Log($"OneButtonOnOnly: Subscribed to {button.name} events");
            }
        }
        
        // Set default selection
        if (defaultSelectedButton != null && toggleButtons.Contains(defaultSelectedButton))
        {
            SetSelectedButton(defaultSelectedButton);
        }
        
        if (showDebugLogs) Debug.Log($"OneButtonOnOnly initialized with {toggleButtons.Count} buttons");
    }
    
    private void FindToggleButtonsAutomatically()
    {
        // Find all ToggleButton components in children
        ToggleButton[] foundButtons = GetComponentsInChildren<ToggleButton>();
        toggleButtons.AddRange(foundButtons);
        
        // Also search for common button names in the scene
        string[] buttonNames = { "TalkButton", "MeditateButton", "DanceButton", "ChatButton" };
        
        foreach (string buttonName in buttonNames)
        {
            GameObject buttonObj = GameObject.Find(buttonName);
            if (buttonObj != null)
            {
                ToggleButton toggleButton = buttonObj.GetComponent<ToggleButton>();
                if (toggleButton != null && !toggleButtons.Contains(toggleButton))
                {
                    toggleButtons.Add(toggleButton);
                }
            }
        }
        
        if (showDebugLogs) Debug.Log($"OneButtonOnOnly: Auto-found {toggleButtons.Count} buttons");
    }
    
    public void OnButtonToggledOn(ToggleButton toggledButton)
    {
        if (isHandlingToggle) return; // Prevent infinite loops
        
        if (showDebugLogs) Debug.Log($"OneButtonOnOnly: {toggledButton.name} toggled ON");
        
        // Start the coroutine to handle sequential deactivation/activation
        StartCoroutine(HandleExclusiveToggleOn(toggledButton));
    }
    
    private IEnumerator HandleExclusiveToggleOn(ToggleButton newButton)
    {
        isHandlingToggle = true;
        
        // STEP 1: Turn off the currently selected button FIRST (if different)
        if (currentlySelectedButton != null && currentlySelectedButton != newButton)
        {
            if (showDebugLogs) Debug.Log($"OneButtonOnOnly: Deactivating previous button {currentlySelectedButton.name}");
            
            // Temporarily allow the off event to process
            isHandlingToggle = false;
            currentlySelectedButton.SetToggle(false);
            
            // Wait one frame to ensure the off event processes completely
            yield return null;
            
            // Resume handling to prevent other buttons from interfering
            isHandlingToggle = true;
        }
        
        // STEP 2: Turn off any other buttons that might be on
        foreach (ToggleButton button in toggleButtons)
        {
            if (button != null && button != newButton && button.isToggled)
            {
                if (showDebugLogs) Debug.Log($"OneButtonOnOnly: Turning off {button.name}");
                
                // Temporarily allow the off event to process
                isHandlingToggle = false;
                button.SetToggle(false);
                
                // Wait one frame to ensure the off event processes
                yield return null;
                
                // Resume handling
                isHandlingToggle = true;
            }
        }
        
        // STEP 3: Set the new currently selected button
        currentlySelectedButton = newButton;
        
        if (showDebugLogs) Debug.Log($"OneButtonOnOnly: {newButton.name} is now the active button");
        
        isHandlingToggle = false;
    }
    
    public void OnButtonToggledOff(ToggleButton toggledButton)
    {
        if (isHandlingToggle) return; // Prevent infinite loops
        
        if (showDebugLogs) Debug.Log($"OneButtonOnOnly: {toggledButton.name} toggled OFF");
        
        isHandlingToggle = true;
        
        // If we don't allow all buttons to be off, turn this button back on
        if (!allowAllOff)
        {
            toggledButton.SetToggle(true);
            currentlySelectedButton = toggledButton;
            if (showDebugLogs) Debug.Log($"OneButtonOnOnly: {toggledButton.name} forced back on (allowAllOff = false)");
        }
        else
        {
            // Allow the button to be turned off
            if (currentlySelectedButton == toggledButton)
            {
                currentlySelectedButton = null;
                if (showDebugLogs) Debug.Log($"OneButtonOnOnly: No button is now active");
            }
        }
        
        isHandlingToggle = false;
    }
    
    // Public methods for external control
    public void SetSelectedButton(ToggleButton button)
    {
        if (button != null && toggleButtons.Contains(button))
        {
            if (showDebugLogs) Debug.Log($"OneButtonOnOnly: Manually setting {button.name} as selected");
            
            StartCoroutine(HandleSetSelectedButton(button));
        }
    }
    
    private IEnumerator HandleSetSelectedButton(ToggleButton button)
    {
        isHandlingToggle = true;
        
        // Turn off all buttons first
        foreach (ToggleButton tb in toggleButtons)
        {
            if (tb != null && tb.isToggled)
            {
                isHandlingToggle = false;
                tb.SetToggle(false);
                yield return null;
                isHandlingToggle = true;
            }
        }
        
        // Turn on the selected button
        isHandlingToggle = false;
        button.SetToggle(true);
        yield return null;
        
        currentlySelectedButton = button;
    }
    
    public void SetSelectedButtonByName(string buttonName)
    {
        ToggleButton button = toggleButtons.Find(tb => tb != null && tb.name.Contains(buttonName));
        if (button != null)
        {
            SetSelectedButton(button);
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning($"OneButtonOnOnly: Button with name containing '{buttonName}' not found");
        }
    }
    
    public ToggleButton GetSelectedButton()
    {
        return currentlySelectedButton;
    }
    
    public string GetSelectedButtonName()
    {
        return currentlySelectedButton != null ? currentlySelectedButton.name : "None";
    }

    
    // Add a button to the group at runtime
    public void AddButton(ToggleButton button)
    {
        if (button != null && !toggleButtons.Contains(button))
        {
            toggleButtons.Add(button);
            button.OnToggleOn.AddListener(() => OnButtonToggledOn(button));
            button.OnToggleOff.AddListener(() => OnButtonToggledOff(button));
            
            if (showDebugLogs) Debug.Log($"OneButtonOnOnly: Added {button.name} to group");
        }
    }
    
    // Remove a button from the group
    public void RemoveButton(ToggleButton button)
    {
        if (button != null && toggleButtons.Contains(button))
        {
            toggleButtons.Remove(button);
            button.OnToggleOn.RemoveListener(() => OnButtonToggledOn(button));
            button.OnToggleOff.RemoveListener(() => OnButtonToggledOff(button));
            
            if (currentlySelectedButton == button)
            {
                currentlySelectedButton = null;
            }
            
            if (showDebugLogs) Debug.Log($"OneButtonOnOnly: Removed {button.name} from group");
        }
    }
    
    // Force all buttons off (useful for resetting state)
    public void ClearSelection()
    {
        if (showDebugLogs) Debug.Log("OneButtonOnOnly: Clearing all button selections");
        
        StartCoroutine(HandleClearSelection());
    }
    
    private IEnumerator HandleClearSelection()
    {
        isHandlingToggle = true;
        
        foreach (ToggleButton button in toggleButtons)
        {
            if (button != null && button.isToggled)
            {
                isHandlingToggle = false;
                button.SetToggle(false);
                yield return null;
                isHandlingToggle = true;
                
            }
        }
        
        currentlySelectedButton = null;
        isHandlingToggle = false;
    }
    
}
