using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using PassthroughCameraSamples;
using System.Collections;
using UnityEngine.Video; // Add this using directive

public class VideoBehaviour : MonoBehaviour
{
    [SerializeField] private string _videoFilePath = "Videos/people-detection";
    [SerializeField] private bool _loopVideo = true;
            
    private Texture2D _videoFrameTexture;
    private VideoPlayer _videoPlayer;
    private RenderTexture _videoRenderTexture;
    
    private Transform _spawnPosition;

    public void PlayVideo(Transform spawnPosition)
    {
        _spawnPosition = spawnPosition;
        StartVideoPlayback();
    }

    private void StartVideoPlayback()
    {
        StartCoroutine(PlayVideoCoroutine());
    }
    
    private IEnumerator PlayVideoCoroutine()
    {
        if (string.IsNullOrEmpty(_videoFilePath))
        {
            Debug.LogWarning("VideoBehaviour: Video file path is not set.");
            yield break;
        }
        
        VideoClip videoClip = Resources.Load<VideoClip>(_videoFilePath);
        
        if (videoClip == null)
        {
            Debug.LogError($"VideoBehaviour: Could not load video from Resources/{_videoFilePath}");
            yield break;
        }

        if (_videoPlayer == null)
        {
            GameObject videoPlayerGO = new("VideoPlayer");
            videoPlayerGO.transform.SetParent(transform);
            _videoPlayer = videoPlayerGO.AddComponent<VideoPlayer>();
        }
        
        _videoPlayer.clip = videoClip;
        _videoPlayer.isLooping = _loopVideo;
        _videoPlayer.playOnAwake = true;
        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        
        int videoWidth = (int)videoClip.width;
        int videoHeight = (int)videoClip.height;
        
        if (_videoRenderTexture != null)
        {
            _videoRenderTexture.Release();
        }
        
        _videoRenderTexture = new RenderTexture(videoWidth, videoHeight, 0, RenderTextureFormat.RGB111110Float);
        _videoPlayer.targetTexture = _videoRenderTexture;
        
        _videoFrameTexture = new Texture2D(_videoRenderTexture.width, _videoRenderTexture.height, TextureFormat.RGB24, false);

        FrameBehaviour.CreateFrameBehaviourFromPrefab(_videoFrameTexture, new Pose(
                _spawnPosition.position,
                Quaternion.LookRotation(-_spawnPosition.forward, Vector3.up)
            ),
            true
        );

        _videoPlayer.Prepare();
        
        while (!_videoPlayer.isPrepared)
        {
            yield return null;
        }
        _videoPlayer.Play();

        yield return new WaitForEndOfFrame();
        
        while (_videoPlayer.isPlaying)
        {            
            CaptureVideoFrame();
            yield return new WaitForSeconds(1f / (float)_videoPlayer.frameRate); // Capture at video frame rate
        }
    }
    
    public void CaptureVideoFrame()
    {
        if (_videoPlayer == null || _videoRenderTexture == null || !_videoPlayer.isPlaying)
        {
            Debug.LogWarning("VideoBehaviour: Video is not playing or RenderTexture is null.");
            return;
        }
        
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = _videoRenderTexture;
        
        _videoFrameTexture.ReadPixels(new Rect(0, 0, _videoRenderTexture.width, _videoRenderTexture.height), 0, 0);
        _videoFrameTexture.Apply();

        RenderTexture.active = previousActive;
    }
    
    public void StopVideo()
    {
        if (_videoPlayer != null && _videoPlayer.isPlaying)
        {
            _videoPlayer.Stop();
        }
    }
        
    private void OnDestroy()
    {
        if (_videoRenderTexture != null)
        {
            _videoRenderTexture.Release();
            _videoRenderTexture = null;
        }
    }
}