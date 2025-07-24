using System;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.UI;
using XrAiAccelerator;

public class FrameBehaviour : MonoBehaviour
{
    [SerializeField] private Grabbable grabbable;
    [SerializeField] private RawImage rawImage;
    private bool _isCollidingWithPad = false;
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

        if (textureAspectRatio > containerAspectRatio)
        {
            float scale = containerAspectRatio / textureAspectRatio;
            float yOffset = (1f - scale) * 0.5f;
            rawImage.uvRect = new Rect(0, yOffset, 1, scale);
        }
        else
        {
            float scale = textureAspectRatio / containerAspectRatio;
            float xOffset = (1f - scale) * 0.5f;
            rawImage.uvRect = new Rect(xOffset, 0, scale, 1);
        }
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
            _collidingPad = null;
        }
    }

    public static FrameBehaviour CreateFrameBehaviourFromPrefab(Texture2D texture, Pose location)
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
        frameObject.transform.position = location.position;
        frameObject.transform.rotation = location.rotation;

        frameBehaviour.SetTexture(texture);

        return frameBehaviour;
    }
}
