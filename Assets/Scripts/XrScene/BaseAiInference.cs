using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using XrAiAccelerator;

public abstract class BaseAiInference<T> : MonoBehaviour
{
    [SerializeField] protected TMP_Text _resultText;
    [SerializeField] protected TMP_Text _statusText;
    [SerializeField] protected TMP_Text _timerText;
    [SerializeField] protected PadBehaviour _pad;
    [SerializeField] protected Transform _newFramePosition;

    protected Task<XrAiResult<T>> _task;
    protected XrAiModelManager _modelManager;
    protected float _startTime;
    protected bool _isTimerActive;
    protected Coroutine _timerCoroutine;
    protected Task _currentTask;
    protected static CancellationTokenSource _cancellationTokenSource;

    protected abstract void Execute(string model, Dictionary<string, string> globalProperties);
    protected abstract void ProcessResult(T data);
    protected virtual void Initialize() { }

    public void Start()
    {
        _modelManager = XrAiModelManager.GetModelManager();
        if (_modelManager == null)
        {
            Debug.LogError("XrAiModelManager not found in the scene.");
        }
        Initialize();
    }

    public void Update()
    {
        if (_task != null)
        {
            if (!_task.IsCompleted) return;

            if (_task.IsFaulted)
            {
                Debug.LogException(_task.Exception);
                _statusText.text = _task.Exception.Message;
                StopTimer();
                _task = null;
                return;
            }

            XrAiResult<T> result = _task.Result;
            if (!result.IsSuccess)
            {
                Debug.LogException(new Exception(result.ErrorMessage));
                _statusText.text = result.ErrorMessage;
                StopTimer();
                _task = null;
                return;
            }

            try
            {
                ProcessResult(result.Data);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _statusText.text = e.Message;
            }
            StopTimer();
            _task = null;
        }
    }

    public void OnClick(string model)
    {
        StartCoroutine(OnClickAsync(model));
    }

    public IEnumerator OnClickAsync(string model)
    {
        yield return null; // Allow UI to update before starting the inference
        _statusText.text = $"Running inference for model: {model}";
        Debug.Log($"Inference: Starting inference for model: {model}");

        StartTimer();

        Debug.Log("Inference: Clearing previous bounding boxes.");
        FrameBehaviour frame = _pad.GetFrame();
        if (frame != null)
        {
            frame.ClearBoundingBoxes();
        }

        Debug.Log("Inference: Cancelling any ongoing tasks.");
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        Debug.Log($"Inference: Starting inference for model: {model}");
        Dictionary<string, string> globalProperties = _modelManager.GetGlobalProperties(model);

        Debug.Log("Inference: Calling Execute method.");
        Execute(model, globalProperties);

        Debug.Log("Inference: Execute method called, waiting for next frame.");
        yield return null;
    }

    protected void StartTimer()
    {
        _startTime = Time.time;
        _isTimerActive = true;
        _timerText.text = "0.00s";
        _timerCoroutine = StartCoroutine(UpdateTimerCoroutine());
    }

    protected void StopTimer()
    {
        _isTimerActive = false;
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
    }

    private IEnumerator UpdateTimerCoroutine()
    {
        while (_isTimerActive)
        {
            float elapsedTime = Time.time - _startTime;
            _timerText.text = $"{elapsedTime:F2}s";
            yield return null;
        }
    }

    protected FrameBehaviour GetFrame()
    {
        return _pad.GetFrame();
    }

    protected Transform GetNewFramePosition()
    {
        return _newFramePosition;
    }

    protected Dictionary<string, string> GetWorkflowProperties(string model, string workflow)
    {
        return _modelManager.GetWorkflowProperties(model, workflow);
    }


    protected FrameBehaviour CreateFrameBehaviourFromPrefab(Texture2D texture)
    {
        return FrameBehaviour.CreateFrameBehaviourFromPrefab(texture, new Pose(
            _newFramePosition.position,
            _newFramePosition.rotation
        ));
    }    

    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }

}
