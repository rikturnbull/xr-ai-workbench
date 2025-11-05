using Oculus.Interaction;
using System;
using UnityEngine;
using UnityEngine.UI;
using XrAiAccelerator;

public class FrameBehaviour : MonoBehaviour
{
    [SerializeField] private Grabbable grabbable;
    [SerializeField] private RawImage rawImage;
    private bool _isCollidingWithPad = false;
    private bool _isVideo = false;
    private PadBehaviour _collidingPad = null;
    private Rigidbody _rb;

    void Start()
    {
        if (grabbable == null)
            grabbable = GetComponent<Grabbable>();

        _rb = GetComponent<Rigidbody>();

        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised += OnPointerEvent;
        }
    }

    public void SetIsVideo(bool isVideo)
    {
        _isVideo = isVideo;
    }

    public bool GetIsVideo()
    {
        return _isVideo;
    }

    private void OnDestroy()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
        }
    }

    private void OnPointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Unselect && grabbable.SelectingPointsCount == 0)
        {
            if (_isCollidingWithPad && _rb != null && _collidingPad != null)
            {
                Transform targetPos = _collidingPad.GetTargetPosition();
                if (targetPos != null)
                {
                    transform.position = targetPos.position;
                    transform.rotation = targetPos.rotation;
                    transform.SetParent(targetPos, true);
                }

                _collidingPad.SetFrame(this);
            }
        }
    }

    public Grabbable GetGrabbable()
    {
        return grabbable;
    }

    public Texture2D GetTexture()
    {
        return rawImage.texture as Texture2D;
    }

    public void ClearBoundingBoxes()
    {
        XrAiObjectDetectorHelper.ClearBoxes(rawImage.transform);
    }

    public void DrawBoundingBoxes(XrAiBoundingBox[] boundingBoxes)
    {
        float scaleX = rawImage.rectTransform.rect.width / rawImage.texture.width;
        float scaleY = rawImage.rectTransform.rect.height / rawImage.texture.height;

        float imageWidth = rawImage.rectTransform.rect.width;
        float imageHeight = rawImage.rectTransform.rect.height;

        XrAiObjectDetectorHelper.DrawBoxes(rawImage.transform, boundingBoxes, new Vector2(scaleX, scaleY), new Vector2(imageWidth, imageHeight));
    }

    public void SetTexture(Texture2D texture)
    {
        if (texture == null || rawImage == null) return;

        rawImage.texture = texture;

        RectTransform rectTransform = rawImage.rectTransform;
        float containerWidth = rectTransform.rect.width;
        float containerHeight = rectTransform.rect.height;

        float textureAspectRatio = (float)texture.width / texture.height;
        float containerAspectRatio = containerWidth / containerHeight;

        rawImage.uvRect = new Rect(0, 0, 1, 1);

        if (textureAspectRatio > containerAspectRatio)
        {
            float newHeight = containerWidth / textureAspectRatio;
            rectTransform.sizeDelta = new Vector2(containerWidth, newHeight);
        }
        else
        {
            float newWidth = containerHeight * textureAspectRatio;
            rectTransform.sizeDelta = new Vector2(newWidth, containerHeight);
        }
    }

    public void OnClose()
    {
        if (_isCollidingWithPad && _collidingPad != null)
        {
            transform.SetParent(null, true);
            _collidingPad.SetFrame(null);
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        PadBehaviour pad = other.GetComponent<PadBehaviour>();
        if (pad != null)
        {
            _isCollidingWithPad = true;
            _collidingPad = pad;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PadBehaviour>() != null)
        {
            _isCollidingWithPad = false;
            if (_collidingPad != null)
            {
                _collidingPad.SetFrame(null);
            }
            transform.SetParent(null, true);
            _collidingPad = null;
        }
    }

    public static FrameBehaviour CreateFrameBehaviourFromPrefab(Texture2D texture, Pose location, bool isVideo = false)
    {
        GameObject framePrefab = Resources.Load<GameObject>("Prefabs/Frame");
        if (framePrefab == null)
        {
            throw new Exception("Frame prefab not found in Resources.");
        }

        GameObject frameObject = Instantiate(framePrefab);
        FrameBehaviour frameBehaviour = frameObject.GetComponent<FrameBehaviour>();
        if (frameBehaviour == null)
        {
            throw new Exception("FrameBehaviour component not found on the instantiated prefab.");
        }
        frameBehaviour.SetIsVideo(isVideo);

        frameObject.transform.position = location.position;
        frameObject.transform.rotation = location.rotation;

        frameBehaviour.SetTexture(texture);

        return frameBehaviour;
    }
}
