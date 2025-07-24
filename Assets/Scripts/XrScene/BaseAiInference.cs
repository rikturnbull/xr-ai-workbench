using System;
using System.Threading.Tasks;
using System.Collections.Generic;
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
    [SerializeField] protected ParticleSystem _loadingParticles;

    protected Task<XrAiResult<T>> _task;
    protected XrAiModelManager _modelManager;
    protected float _startTime;
    protected bool _isTimerActive;

    protected abstract void Execute(string model, Dictionary<string, string> globalProperties);
    protected abstract void ProcessResult(T data);

    public void Start()
    {
        _modelManager = FindFirstObjectByType<XrAiModelManager>();
        if (_modelManager == null)
        {
            Debug.LogError("XrAiModelManager not found in the scene.");
        }
    }

    public void Update()
    {
        // Update timer if active
        if (_isTimerActive)
        {
            float elapsedTime = Time.time - _startTime;
            _timerText.text = $"{elapsedTime:F2}s";
        }

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
        _statusText.text = $"Running inference for model: {model}";
        try
        {
            StartTimer();
            FrameBehaviour frame = _pad.GetFrame();
            if (frame != null)
            {
                frame.ClearBoundingBoxes();
            }

            Dictionary<string, string> globalProperties = _modelManager.GetGlobalProperties(model);
            Execute(model, globalProperties);
        }
        catch (Exception e)
        {
            StopTimer();
            Debug.LogException(e);
            _statusText.text = e.Message;
        }
    }

    protected void StartTimer()
    {
        _loadingParticles.Play();
        _startTime = Time.time;
        _isTimerActive = true;
        _timerText.text = "0.00s";
    }

    protected void StopTimer()
    {
        _loadingParticles.Stop();
        _isTimerActive = false;
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
}
