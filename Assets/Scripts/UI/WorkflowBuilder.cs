using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using XrAiAccelerator;

public class WorkflowBuilder : MonoBehaviour
{
    private XrAiWorkflowInspector _xrAiWorkflowInspector;
    private Dictionary<string, Dictionary<string, WorkflowBuilderPanel>> _workflowPanels = new();
    private WorkflowBuilderMenu _workflowBuilderMenu;

    private void Start()
    {
        _xrAiWorkflowInspector = FindFirstObjectByType<XrAiWorkflowInspector>();
        PrefabManager prefabManager = FindFirstObjectByType<PrefabManager>();
        Dictionary<string, Dictionary<string, Type>> implementations = GetAllImplementations();

        foreach (var implementation in implementations)
        {
            GameObject typeGameObject = new(implementation.Key);
            typeGameObject.transform.parent = transform;
            typeGameObject.transform.localRotation = Quaternion.identity;
            typeGameObject.transform.localPosition = Vector3.zero;
            _workflowPanels[implementation.Key] = CreateWorkflowBuilderPanels(prefabManager, typeGameObject, implementation.Key, implementation.Value);
        }

        _workflowBuilderMenu = CreateWorkflowBuilderMenu(prefabManager, implementations, _workflowPanels);
        ToggleVisibility();
    }

    public void CloseAllPanels()
    {
        foreach (var type in _workflowPanels.Keys)
        {
            foreach (var panel in _workflowPanels[type].Values)
            {
                panel.gameObject.SetActive(false);
            }
        }
    }

    public void ToggleVisibility()
    {
        if (_workflowBuilderMenu.gameObject.activeSelf)
        {
            CloseAllPanels();
        }
        _workflowBuilderMenu.gameObject.SetActive(!_workflowBuilderMenu.gameObject.activeSelf);
    }

    public Dictionary<string, string> GetWorkflowOptions(string typeName, string providerName)
    {
        if (_workflowPanels.ContainsKey(typeName))
        {
            var panels = _workflowPanels[typeName];
            if (panels.ContainsKey(providerName))
            {
                return panels[providerName].GetWorkflowOptions();
            }
        }
        return new Dictionary<string, string>();
    }

    private Dictionary<string, Dictionary<string, Type>> GetAllImplementations()
    {
        Dictionary<string, Dictionary<string, Type>> implementations = new Dictionary<string, Dictionary<string, Type>>();
        List<Type> types = XrAiFactory.XrAiInterfaces.Values.ToList();
        types = types.OrderBy(t => t.Name).ToList();

        foreach (Type type in types)
        {
            Dictionary<string, Type> implementation = XrAiFactory.GetImplementationsForType(type);
            implementations[type.Name] = implementation;
        }
        return implementations;
    }

    private WorkflowBuilderMenu CreateWorkflowBuilderMenu(PrefabManager prefabManager, Dictionary<string, Dictionary<string, Type>> allWorkflows, Dictionary<string, Dictionary<string, WorkflowBuilderPanel>> workflowPanels)
    {
        GameObject workflowBuilderMenuObject = new("WorkflowBuilderMenu");
        workflowBuilderMenuObject.transform.parent = transform;
        workflowBuilderMenuObject.transform.localPosition = Vector3.zero;
        workflowBuilderMenuObject.transform.localRotation = Quaternion.identity;
        WorkflowBuilderMenu workflowBuilderMenu = workflowBuilderMenuObject.AddComponent<WorkflowBuilderMenu>();

        int largestImplementationCount = 0;
        foreach (var workflow in allWorkflows)
        {
            if (workflow.Value.Count > largestImplementationCount)
            {
                largestImplementationCount = workflow.Value.Count;
            }
        }
        workflowBuilderMenu.Init(prefabManager, allWorkflows.Keys.Count, largestImplementationCount);

        foreach (var workflow in allWorkflows)
        {
            if( workflowPanels.ContainsKey(workflow.Key) && workflowPanels[workflow.Key].Count > 0 )
                workflowBuilderMenu.AddWorkflow(prefabManager, this, workflow.Key, workflowPanels[workflow.Key]);
        }
        return workflowBuilderMenu;
    }
    
    private Dictionary<string, WorkflowBuilderPanel> CreateWorkflowBuilderPanels(PrefabManager prefabManager, GameObject parent, string interfaceName, Dictionary<string, Type> providers)
    {
        Dictionary<string, WorkflowBuilderPanel> workflowPanels = new();
        foreach (var provider in providers)
        {
            GameObject go = new($"{provider.Key}");
            go.transform.parent = parent.transform;
            go.transform.localPosition = new Vector3(0, 0, -0.05f);
            go.transform.localRotation = Quaternion.identity;
            go.SetActive(false);

            WorkflowBuilderPanel workflowBuilderPanel = go.AddComponent<WorkflowBuilderPanel>();
            List<XrAiWorkflowInspectorOption> options = _xrAiWorkflowInspector.GetWorkflowInspectorOptions(interfaceName, provider.Key);
            workflowBuilderPanel.Init(prefabManager, provider.Key, options);
            workflowPanels[provider.Key] = workflowBuilderPanel;
        }
        return workflowPanels;
    }
}
