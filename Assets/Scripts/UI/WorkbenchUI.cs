using System.Collections;
using TMPro;
using UnityEngine;

public class WorkbenchUI : MonoBehaviour
{
    public TMP_Text _resultText;
    public TMP_Text _statusText;
    public TMP_Text _timerText;
    public PadBehaviour _pad;
    public Transform _newFramePosition;

    protected float _startTime;
    protected bool _isTimerActive;
    protected Coroutine _timerCoroutine;

    public string GetPromptText()
    {
        return _resultText.text;
    }

    public void SetStatusText(string text)
    {
        _statusText.text = text;
    }

    public void SetResultText(string text)
    {
        _resultText.text = text;
    }

    public void StartTimer()
    {
        _startTime = Time.time;
        _isTimerActive = true;
        _timerText.text = "0.00s";
        _timerCoroutine = StartCoroutine(UpdateTimerCoroutine());
    }

    public void StopTimer()
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
}