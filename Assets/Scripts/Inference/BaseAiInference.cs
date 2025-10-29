using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using XrAiAccelerator;

public abstract class BaseAiInference<T,S> : MonoBehaviour where T : class where S : class
{
    protected Task _currentTask;
    protected static CancellationTokenSource _cancellationTokenSource;

    private Dictionary<string, T> _loadedProviders = new();
    private XrAiSecretsManager _secretsManager;
    private WorkbenchUI _workbenchUI;
    private WorkflowBuilder _workflowBuilder;

    public void Start()
    {
        _workflowBuilder = FindFirstObjectByType<WorkflowBuilder>();
        if (_workflowBuilder == null)
        {
            Debug.LogError($"WorkflowBuilder not found in the scene.");
        }
        _secretsManager = XrAiSecretsManager.GetSecretsManager();
        if (_secretsManager == null)
        {
            Debug.LogError($"XrAiSecretsManager not found in the scene.");
        }
        _workbenchUI = FindFirstObjectByType<WorkbenchUI>();
        if (_workbenchUI == null)
        {
            Debug.LogError($"WorkbenchUI not found in the scene.");
        }

        SetUp();
    }

    protected virtual void SetUp() { }

    protected abstract T LoadProvider(string provider);

    protected abstract IEnumerator InitializeProvider(T loadedProvider, Dictionary<string, string> globalProperties);

    protected abstract IEnumerator ExecuteProvider(T loadedProvider, string provider, Dictionary<string, string> globalProperties, Action<XrAiResult<S>> callback);

    private IEnumerator ExecuteCoroutine(string provider, Dictionary<string, string> globalProperties)
    {
        yield return null;

        T loadedProvider;
        bool init = true;
        try
        {
            if (_loadedProviders.ContainsKey(provider))
            {
                loadedProvider = _loadedProviders[provider];
                init = false;
            }
            else
            {
                loadedProvider = LoadProvider(provider);
                if (loadedProvider == null) yield break;
                _loadedProviders[provider] = loadedProvider;
            }
        }
        catch (Exception e)
        {
            StopTimer();
            SetStatusText($"Error loading provider {provider}: {e.Message}");
            Debug.LogException(e);
            yield break;
        }

        yield return null;
        if (init)
        {
            yield return InitializeProvider(loadedProvider, globalProperties);
        }

        yield return null;
        yield return ExecuteProvider(loadedProvider, provider, GetWorkflowOptions(provider), OnResult);
    }

    protected void OnResult(XrAiResult<S> result)
    {
        StopTimer();
        if (!result.IsSuccess)
        {
            Debug.LogException(new Exception(result.ErrorMessage));
            SetStatusText(result.ErrorMessage);
            return;
        }

        if (result.Data == null)
        {
            SetStatusText("Received empty data.");
            return;
        }

        SetStatusText("Inference completed successfully.");

        OnInferenceResult(result);
    }

    protected abstract void OnInferenceResult(XrAiResult<S> result);

    public void OnClick(string provider)
    {
        StartCoroutine(OnClickAsync(provider));
    }

    public IEnumerator OnClickAsync(string provider)
    {
        _workbenchUI._statusText.text = $"Running inference for provider: {provider}";
                        
        StartTimer();

        FrameBehaviour frame = _workbenchUI._pad.GetFrame();
        if (frame != null)
        {
            frame.ClearBoundingBoxes();
        }

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        
        Dictionary<string, string> properties = new()
        {
            { "apiKey", _secretsManager.GetSecret(provider) }
        };

        yield return StartCoroutine(ExecuteCoroutine(provider, properties));
    }

    protected void StartTimer() => _workbenchUI.StartTimer();

    protected void StopTimer() => _workbenchUI.StopTimer();

    protected string GetPromptText() => _workbenchUI.GetPromptText();

    protected void SetStatusText(string text) => _workbenchUI.SetStatusText(text);

    protected void SetResultText(string text) => _workbenchUI.SetResultText(text);

    protected FrameBehaviour GetFrame() => _workbenchUI._pad.GetFrame();

    protected Transform GetNewFramePosition() => _workbenchUI._newFramePosition;

    protected Dictionary<string, string> GetWorkflowOptions(string provider) => _workflowBuilder.GetWorkflowOptions(typeof(T).Name, provider);

    protected FrameBehaviour CreateFrameBehaviourFromPrefab(Texture2D texture)
    {
        return FrameBehaviour.CreateFrameBehaviourFromPrefab(texture, new Pose(
            _workbenchUI._newFramePosition.position,
            _workbenchUI._newFramePosition.rotation
        ));
    }    

    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }

}
