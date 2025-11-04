using System;
using System.Collections.Generic;
using UnityEngine;
using XrOpenAI;
using XrOpenAI.Data.Common;
using Newtonsoft.Json;

public class UITool
{
    private OpenAIRealtimeClient _openAiRealTimeClient;
    private Dictionary<string, MenuController> _menuControllers;

    public UITool(OpenAIRealtimeClient openAiRealTimeClient)
    {
        _openAiRealTimeClient = openAiRealTimeClient;
    }

    public Dictionary<string, Action<string,string>> GetHandlers()
    {
        return new Dictionary<string, Action<string,string>>
        {
            { "RunProvider", (callId, args) => RunProvider(callId, args) },
            { "OpenWorkflowMenu", (callId, args) => OpenWorkflowMenu(callId, args) }
        };
    }

    public List<Tool> GetTools(Dictionary<string, MenuController> menuControllers)
    {
        _menuControllers = menuControllers;
        return new List<Tool>
        {
            new() {
                Name = "OpenWorkflowMenu",
                Description = "Opens the workflow menu specified by the user.",
                Type = "function",
                Parameters = new Parameter
                {
                    Type = "object",
                    Properties = new Dictionary<string, Property>
                    {
                        {
                            "workflowName", new Property
                            {
                                Type = "string",
                                Description = "Workflow name to open. For example: 'TextToImage', 'SpeechToText', 'ImageToText'."
                            }
                        }
                    },
                    Required = new List<string> { "workflowName" }
                }
            },
            new() {
                Name = "RunProvider",
                Description = "Runs the provider specified by the user.",
                Type = "function",
                Parameters = new Parameter
                {
                    Type = "object",
                    Properties = new Dictionary<string, Property>
                    {
                        {
                            "providerName", new Property
                            {
                                Type = "string",
                                Description = "Provider name to run. For example: 'Google', 'OpenAI', 'Groq'."
                            }
                        }
                    },
                    Required = new List<string> { "providerName" }
                }
            }
        };
    }

    private void RunProvider(string callId, string arguments)
    {
        try
        {
            RunProviderArgs args = JsonConvert.DeserializeObject<RunProviderArgs>(arguments);

            if (string.IsNullOrEmpty(args?.ProviderName.ToLower()))
            {
                Debug.LogWarning("UITool: RunProvider called with invalid arguments.");
                return;
            }
            foreach(var menuController in _menuControllers)
            {
                if(menuController.Value.IsMenuOpen())
                {
                    menuController.Value.ClickProvider(args.ProviderName);
                }
            }
            _ = _openAiRealTimeClient.SendFunctionCallResult(callId, $"Run provider '{args.ProviderName}'. Confirm.");

        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void OpenWorkflowMenu(string callId, string arguments)
    {
        try
        {
            OpenWorkflowMenuArgs args = JsonConvert.DeserializeObject<OpenWorkflowMenuArgs>(arguments);

            if (string.IsNullOrEmpty(args?.WorkflowName.ToLower()))
            {
                Debug.LogWarning("UITool: OpenWorkflowMenu called with invalid arguments.");
                return;
            }
            if (_menuControllers != null && _menuControllers.TryGetValue(args.WorkflowName.ToLower(), out MenuController controller))
            {
                controller.ToggleMenu();
            }
            else
            {
                Debug.LogWarning($"UITool: No MenuController found for workflow '{args.WorkflowName}'.");
            }
            _ = _openAiRealTimeClient.SendFunctionCallResult(callId, $"Opened workflow menu '{args.WorkflowName}'");

        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    [Serializable]
    private class RunProviderArgs
    {
        [JsonProperty("providerName")]
        public string ProviderName { get; set; }
    }

    [Serializable]
    private class OpenWorkflowMenuArgs
    {
        [JsonProperty("workflowName")]
        public string WorkflowName { get; set; }
    }
}
