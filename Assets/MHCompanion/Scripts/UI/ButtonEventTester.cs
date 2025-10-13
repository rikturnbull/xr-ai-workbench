using UnityEngine;

public class ButtonEventTester : MonoBehaviour
{
    // Specific methods for each button type
    public void OnChatButtonPressed()
    {
        Debug.Log("💬 Chat Button Activated - Starting conversation...");
    }
    
    public void OnChatButtonReleased()
    {
        Debug.Log("💬 Chat Button Deactivated - Ending conversation...");
    }
    
    public void OnMeditateButtonPressed()
    {
        Debug.Log("🧘 Meditate Button Activated - Starting meditation session...");
    }
    
    public void OnMeditateButtonReleased()
    {
        Debug.Log("🧘 Meditate Button Deactivated - Ending meditation session...");
    }
    
    public void OnDanceButtonPressed()
    {
        Debug.Log("💃 Dance Button Activated - Starting dance routine...");
    }
    
    public void OnDanceButtonReleased()
    {
        Debug.Log("💃 Dance Button Deactivated - Ending dance routine...");
    }
}