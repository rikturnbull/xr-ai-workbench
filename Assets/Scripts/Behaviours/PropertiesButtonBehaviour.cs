using UnityEngine;

public class PropertiesButtonBehaviour : MonoBehaviour
{
    [SerializeField] private Transform _handle;

    private WorkflowBuilder _workflowBuilder;

    private void Start()
    {
        _workflowBuilder = FindFirstObjectByType<WorkflowBuilder>();
    }

    public void OnClick()
    {
        if (_workflowBuilder != null)
        {
            _workflowBuilder.ToggleVisibility();
        }
    }
}
