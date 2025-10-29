using UnityEngine;
using Oculus.Interaction;

public class ButtonVisualFeedback : MonoBehaviour
{
    [Header("Visual Elements")]
    public MeshRenderer buttonBackground;
    public MeshRenderer buttonIcon;
    
    [Header("Materials for Different States")]
    public Material normalMaterial;
    public Material hoverMaterial;
    public Material selectMaterial;
    
    [Header("Colors for Different States")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.cyan;
    public Color selectColor = Color.green;
    
    [Header("Animation Settings")]
    public float transitionDuration = 0.2f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Scale Animation")]
    public Vector3 normalScale = Vector3.one;
    public Vector3 pressedScale = new Vector3(0.9f, 0.9f, 0.9f);
    
    private PokeInteractable pokeInteractable;
    private InteractableUnityEventWrapper eventWrapper;
    private Coroutine currentTransition;
    private Material originalMaterial;
    
    void Start()
    {
        pokeInteractable = GetComponent<PokeInteractable>();
        eventWrapper = GetComponent<InteractableUnityEventWrapper>();
        
        if (buttonBackground != null)
        {
            originalMaterial = buttonBackground.material;
        }
        
        // Subscribe to interaction events
        if (eventWrapper != null)
        {
            eventWrapper.WhenHover.AddListener(OnHover);
            eventWrapper.WhenUnhover.AddListener(OnUnhover);
            eventWrapper.WhenSelect.AddListener(OnSelect);
            eventWrapper.WhenUnselect.AddListener(OnUnselect);
        }
    }
    
    void OnHover()
    {
        SetVisualState(ButtonState.Hover);
    }
    
    void OnUnhover()
    {
        SetVisualState(ButtonState.Normal);
    }
    
    void OnSelect()
    {
        SetVisualState(ButtonState.Select);
        StartCoroutine(ScaleAnimation(pressedScale));
    }
    
    void OnUnselect()
    {
        Debug.Log("OnUnselect called");
        SetVisualState(ButtonState.Hover); // Return to hover if still hovering
        // SetVisualState(ButtonState.Normal); // Return to normal on unselect
        StartCoroutine(ScaleAnimation(normalScale));
    }
    
    void SetVisualState(ButtonState state)
    {
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }
        
        switch (state)
        {
            case ButtonState.Normal:
                currentTransition = StartCoroutine(TransitionToMaterial(normalMaterial, normalColor));
                break;
            case ButtonState.Hover:
                currentTransition = StartCoroutine(TransitionToMaterial(hoverMaterial, hoverColor));
                break;
            case ButtonState.Select:
                currentTransition = StartCoroutine(TransitionToMaterial(selectMaterial, selectColor));
                break;
        }
    }
    
    System.Collections.IEnumerator TransitionToMaterial(Material targetMaterial, Color targetColor)
    {
        if (buttonBackground == null) yield break;
        
        Color startColor = buttonBackground.material.color;
        float elapsed = 0f;
        
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = transitionCurve.Evaluate(elapsed / transitionDuration);
            
            // Transition color
            Color currentColor = Color.Lerp(startColor, targetColor, t);
            buttonBackground.material.color = currentColor;
            
            yield return null;
        }
        
        // Apply final material if specified
        if (targetMaterial != null)
        {
            buttonBackground.material = targetMaterial;
        }
        
        buttonBackground.material.color = targetColor;
    }
    
    System.Collections.IEnumerator ScaleAnimation(Vector3 targetScale)
    {
        if (buttonBackground == null) yield break;
        
        Vector3 startScale = buttonBackground.transform.localScale;
        float elapsed = 0f;
        
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = transitionCurve.Evaluate(elapsed / transitionDuration);
            
            Vector3 currentScale = Vector3.Lerp(startScale, targetScale, t);
            buttonBackground.transform.localScale = currentScale;
            
            yield return null;
        }
        
        buttonBackground.transform.localScale = targetScale;
    }
    
    enum ButtonState
    {
        Normal,
        Hover,
        Select
    }
}

