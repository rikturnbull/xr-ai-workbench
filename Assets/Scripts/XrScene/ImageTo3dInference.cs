using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Siccity.GLTFUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.Async;
using XrAiAccelerator;

public class ImageTo3dInference : BaseAiInference<byte[]>
{
    protected override void Execute(string model, Dictionary<string, string> globalProperties)
    {
        StartCoroutine(ExecuteCoroutine(model, globalProperties));
    }

    private IEnumerator ExecuteCoroutine(string model, Dictionary<string, string> globalProperties)
    {
        IXrAiImageTo3d imageTo3d = XrAiFactory.LoadImageTo3d(model);
        imageTo3d.Initialize(globalProperties);
        if (_cancellationTokenSource.Token.IsCancellationRequested) yield break;

        _currentTask = imageTo3d.Execute(
            GetFrame().GetTexture(),
            GetWorkflowProperties(model, XrAiFactory.WORKFLOW_IMAGE_TO_3D),
            OnImageTo3dResult
        ).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    private void OnImageTo3dResult(XrAiResult<byte[]> result)
    {
        StopTimer();

        if (!result.IsSuccess)
        {
            Debug.LogException(new Exception(result.ErrorMessage));
            _statusText.text = result.ErrorMessage;
            return;
        }

        ProcessResult(result.Data);
    }

    protected override void ProcessResult(byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            throw new ArgumentException("Received empty data for 3D model.");
        }

        GameObject gameObject = Importer.LoadFromBytes(data);
        if (gameObject != null)
        {
            Transform instantiatePosition = GetNewFramePosition();
            gameObject.transform.position = instantiatePosition.position;
            gameObject.transform.rotation = instantiatePosition.rotation;
            gameObject.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            SetupGrabbableObject(gameObject.transform.GetChild(0).gameObject);
        }
        else
        {
            throw new Exception("Failed to load 3D model from data.");
        }
    }

    private void SetupGrabbableObject(GameObject obj)
    {
        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.useGravity = false;
        rb.isKinematic = true;

        MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
        meshCollider.convex = true;

        Grabbable grabbable = obj.AddComponent<Grabbable>();
        grabbable.InjectOptionalRigidbody(rb);
        grabbable.InjectOptionalTargetTransform(obj.transform);

        HandGrabInteractable handGrabInteractable = obj.AddComponent<HandGrabInteractable>();
        handGrabInteractable.InjectOptionalPointableElement(grabbable);
        handGrabInteractable.InjectRigidbody(rb);

        GrabInteractable grabInteractable = obj.AddComponent<GrabInteractable>();
        grabInteractable.InjectOptionalPointableElement(grabbable);
        grabInteractable.InjectRigidbody(rb);

        obj.layer = LayerMask.NameToLayer("Default");
    }

}
