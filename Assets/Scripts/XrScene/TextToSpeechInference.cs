using System;
using System.Collections.Generic;
using UnityEngine;
using XrAiAccelerator;

public class TextToSpeechInference : BaseAiInference<AudioClip>
{
    [SerializeField] private AudioSource _audioSource;

    protected override void Execute(string model, Dictionary<string, string> globalProperties)
    {
        if (string.IsNullOrEmpty(_resultText.text))
        {
            throw new ArgumentException("No text to convert to speech.");
        }

        IXrAiTextToSpeech textToSpeech = XrAiFactory.LoadTextToSpeech(model, globalProperties);
        _task = textToSpeech.Execute(_resultText.text, GetWorkflowProperties(model, XrAiModelManager.WORKFLOW_TEXT_TO_SPEECH));
    }

    protected override void ProcessResult(AudioClip data)
    {
        if (data == null)
        {
            throw new ArgumentException("Received empty audio data.");
        }

        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }

        if (_audioSource != null)
        {
            _audioSource.PlayOneShot(data);
            _statusText.text = "Playing audio...";
        }
        else
        {
            _statusText.text = "Audio generated successfully (no AudioSource found).";
        }
    }
}
