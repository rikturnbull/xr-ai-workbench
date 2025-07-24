using UnityEngine;

public class ImageCopyBehaviour : MonoBehaviour
{
    [SerializeField] protected PadBehaviour _pad;
    [SerializeField] protected Transform _newFramePosition;

    public void OnClick()
    {
        Texture2D inputTexture = _pad.GetFrame().GetTexture();
        if (inputTexture == null)
        {
            Debug.LogError("Input image frame is not set.");
            return;
        }

        FrameBehaviour.CreateFrameBehaviourFromPrefab(inputTexture, new Pose(
            _newFramePosition.position,
            _newFramePosition.rotation
        ));
    }
}
