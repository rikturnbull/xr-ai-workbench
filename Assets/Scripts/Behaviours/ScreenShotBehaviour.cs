using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using PassthroughCameraSamples;
using System.Collections;

public class ScreenShotBehaviour : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds1 = new(1f);

    [SerializeField] private Transform _spawnPosition;
    [SerializeField] private RawImage _rawImage;
    [SerializeField] private WebCamTextureManager _webCamTextureManager;    
    [SerializeField] private string _resourceImagePath = "SampleImages/test-image";

    private bool _loadVideo = true;

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
            if (_loadVideo)
                LoadVideo();
            else
                LoadResourceImage();
            _loadVideo = !_loadVideo;
        }
        else
        {
            yield return _waitForSeconds1;
            if (_loadVideo)
                LoadVideo();
            else
                LoadResourceImage();
            _loadVideo = !_loadVideo;
            // LoadWebCamTexture();
        }

    }

    public void LoadWebCamTexture()
    {
        var webCamTexture = _webCamTextureManager.WebCamTexture;

        if (!webCamTexture.isPlaying)
        {
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

        Color32[] pixels = screenshot.GetPixels32();
        float brightnessFactor = 1.3f; // Adjust this value to control brightness

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i].r = (byte)Mathf.Clamp01(pixels[i].r * brightnessFactor);
            pixels[i].g = (byte)Mathf.Clamp01(pixels[i].g * brightnessFactor);
            pixels[i].b = (byte)Mathf.Clamp01(pixels[i].b * brightnessFactor);
        }

        screenshot.SetPixels32(pixels);
        screenshot.Apply();

        FrameBehaviour.CreateFrameBehaviourFromPrefab(screenshot, new Pose(
                _spawnPosition.position,
                Quaternion.LookRotation(-_spawnPosition.forward, Vector3.up)
            )
        );
    }
    
    public void LoadVideo()
    {
        GameObject videoPlayerObject = new("VideoPlayer");
        VideoBehaviour videoBehaviour = videoPlayerObject.AddComponent<VideoBehaviour>();
        videoBehaviour.PlayVideo(_spawnPosition);
    }

    public void LoadResourceImage()
    {
        if (string.IsNullOrEmpty(_resourceImagePath))
        {
            Debug.LogWarning("ScreenShotBehaviour: Resource image path is not set.");
            return;
        }

        Texture2D resourceTexture = Resources.Load<Texture2D>(_resourceImagePath);

        if (resourceTexture == null)
        {
            Debug.LogError($"ScreenShotBehaviour: Could not load image from Resources/{_resourceImagePath}");
            return;
        }

        Texture2D loadedTexture = new(resourceTexture.width, resourceTexture.height, resourceTexture.format, false);
        loadedTexture.SetPixels(resourceTexture.GetPixels());
        loadedTexture.Apply();

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
