using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using PassthroughCameraSamples;
using System.Collections;

public class ScreenShotBehaviour : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Transform _spawnPosition;
    [SerializeField] private RawImage _rawImage;

    [Header("Camera Reference")]
    [SerializeField] private WebCamTextureManager _webCamTextureManager;
    
    [Header("Controller Input")]
    [SerializeField] private XRNode _inputSource = XRNode.RightHand;
    [SerializeField] private InputFeatureUsage<bool> _buttonInput = CommonUsages.triggerButton;
    [SerializeField] private InputFeatureUsage<bool> _palmMenuButton = CommonUsages.menuButton;
    
    [Header("Screenshot Settings")]
    [SerializeField] private bool _autoProcessWithImageToText = true;
    
    [Header("Resource Settings")]
    [SerializeField] private string _resourceImagePath = "SampleImages/test-image";
    
    [Header("Debug")]
    [SerializeField] private bool _enableDebugLogs = true;
    
    public System.Action<Texture2D> OnScreenshotCaptured;
    
    private void Awake()
    {
        if (_webCamTextureManager == null)
        {
            _webCamTextureManager = FindFirstObjectByType<WebCamTextureManager>();
        }
    }

    public void CaptureScreenshot()
    {
        StartCoroutine(CaptureScreenshotCoroutine());
    }
    
    private IEnumerator CaptureScreenshotCoroutine()
    {    
        if (_webCamTextureManager == null || _webCamTextureManager.WebCamTexture == null)
        {
            if (_enableDebugLogs)
                Debug.LogWarning("ScreenShotBehaviour: WebCamTexture is not available for screenshot.");
            LoadResourceImage();
        }
        else
        {
            yield return new WaitForSeconds(1f);
            LoadWebCamTexture();
        }

    }

    public void LoadWebCamTexture()
    {
        var webCamTexture = _webCamTextureManager.WebCamTexture;

        if (!webCamTexture.isPlaying)
        {
            if (_enableDebugLogs)
                Debug.LogWarning("ScreenShotBehaviour: WebCamTexture is not playing.");
            return;
        }

        Texture2D screenshot = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);

        RenderTexture tempRT = RenderTexture.GetTemporary(webCamTexture.width, webCamTexture.height, 0, RenderTextureFormat.RGB111110Float);

        Graphics.Blit(webCamTexture, tempRT);

        RenderTexture.active = tempRT;
        screenshot.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        screenshot.Apply();
        RenderTexture.active = null;

        RenderTexture.ReleaseTemporary(tempRT);

        Color[] pixels = screenshot.GetPixels();
        float brightnessFactor = 1.3f; // Adjust this value to control brightness
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i].r = Mathf.Clamp01(pixels[i].r * brightnessFactor);
            pixels[i].g = Mathf.Clamp01(pixels[i].g * brightnessFactor);
            pixels[i].b = Mathf.Clamp01(pixels[i].b * brightnessFactor);
        }
        
        screenshot.SetPixels(pixels);
        screenshot.Apply();

        if (_enableDebugLogs)
            Debug.Log($"ScreenShotBehaviour: Screenshot captured! Resolution: {screenshot.width}x{screenshot.height}");

        FrameBehaviour.CreateFrameBehaviourFromPrefab(screenshot, new Pose(
                _spawnPosition.position,
                Quaternion.LookRotation(-_spawnPosition.forward, Vector3.up)
            )
        );
    }
    
    public void LoadResourceImage()
    {
        if (string.IsNullOrEmpty(_resourceImagePath))
        {
            if (_enableDebugLogs)
                Debug.LogWarning("ScreenShotBehaviour: Resource image path is not set.");
            return;
        }
        
        Texture2D resourceTexture = Resources.Load<Texture2D>(_resourceImagePath);
        
        if (resourceTexture == null)
        {
            if (_enableDebugLogs)
                Debug.LogError($"ScreenShotBehaviour: Could not load image from Resources/{_resourceImagePath}");
            return;
        }
        
        Texture2D loadedTexture = new Texture2D(resourceTexture.width, resourceTexture.height, resourceTexture.format, false);
        loadedTexture.SetPixels(resourceTexture.GetPixels());
        loadedTexture.Apply();

        if (_enableDebugLogs)
            Debug.Log($"ScreenShotBehaviour: Loaded resource image! Resolution: {loadedTexture.width}x{loadedTexture.height}");
 
        FrameBehaviour.CreateFrameBehaviourFromPrefab(loadedTexture, new Pose(
                _spawnPosition.position,
                Quaternion.LookRotation(-_spawnPosition.forward, Vector3.up)
            )
        );
  
        OnScreenshotCaptured?.Invoke(loadedTexture);
    }
 
    private void OnDestroy()
    {
        OnScreenshotCaptured = null;
    }
}
