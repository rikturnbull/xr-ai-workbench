using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Siccity.GLTFUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Async;
using XrAiAccelerator;

public class ImageTo3dInference : BaseAiInference<IXrAiImageTo3d, byte[]>
{
    protected override IXrAiImageTo3d LoadProvider(string provider)
    {
        return XrAiFactory.LoadImageTo3d(provider);
    }

    protected override IEnumerator InitializeProvider(IXrAiImageTo3d loadedProvider, Dictionary<string, string> options)
    {
        _currentTask = loadedProvider.Initialize(options).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    protected override IEnumerator ExecuteProvider(IXrAiImageTo3d loadedProvider, string provider, Dictionary<string, string> options, Action<XrAiResult<byte[]>> callback)
    {
        if (_cancellationTokenSource.Token.IsCancellationRequested) yield break;

        Texture2D texture = GetTexture();
        if (texture == null)
        {
            callback(XrAiResult.Failure<byte[]>("No texture available for inference."));
            yield break;
        }

        _currentTask = loadedProvider.Execute(
            texture,
            options,
            callback
        ).WithCancellation(_cancellationTokenSource.Token);
        yield return new WaitUntil(() => _currentTask.IsCompleted || _cancellationTokenSource.Token.IsCancellationRequested);
    }

    protected override void OnInferenceResult(XrAiResult<byte[]> result)
    {
        GameObject gameObject = Importer.LoadFromBytes(result.Data);
        if (gameObject != null)
        {
            Transform instantiatePosition = GetNewFramePosition();
            gameObject.transform.position = instantiatePosition.position;
            gameObject.transform.rotation = instantiatePosition.rotation;
            gameObject.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            FixMaterials(gameObject);

            SetupGrabbableObject(gameObject.transform.GetChild(0).gameObject);
        }
        else
        {
            SetStatusText("Failed to load 3D model from data.");
        }
    }

    private void FixMaterials(GameObject obj)
    {
        MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer renderer in renderers)
        {
            renderer.receiveShadows = false;
            foreach (Material mat in renderer.materials)
            {
                if (mat.HasProperty("_Metallic"))
                    mat.SetFloat("_Metallic", 0f);

                if (mat.HasProperty("_Roughness"))
                    mat.SetFloat("_Roughness", 1.0f);

                if (mat.HasProperty("_Glossiness"))
                    mat.SetFloat("_Glossiness", 0.5f);
                
                if (mat.HasProperty("_Mode"))
                    mat.SetFloat("_Mode", 0);
            }
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
