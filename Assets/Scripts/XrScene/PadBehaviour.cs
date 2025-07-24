using UnityEngine;
using Oculus.Interaction;

public class PadBehaviour : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material onMaterial;
    [SerializeField] private Transform targetPosition;

    private FrameBehaviour _frame;

    void Start()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();
        
        if (meshRenderer != null && defaultMaterial != null)
            meshRenderer.material = defaultMaterial;
    }

    public FrameBehaviour GetFrame()
    {
        return _frame;
    }

    public void SetFrame(FrameBehaviour frame)
    {
        _frame = frame;
    }

    public Transform GetTargetPosition()
    {
        return targetPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<FrameBehaviour>() != null && meshRenderer != null && onMaterial != null)
            meshRenderer.material = onMaterial;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<FrameBehaviour>() != null && meshRenderer != null && defaultMaterial != null)
            meshRenderer.material = defaultMaterial;
    }
}
