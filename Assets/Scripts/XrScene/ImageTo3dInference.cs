using UnityEngine;
using System;
using System.Collections.Generic;
using Siccity.GLTFUtility;
using Oculus.Interaction;
using XrAiAccelerator;

public class ImageTo3dInference : BaseAiInference<byte[]>
{
    protected override void Execute(string model, Dictionary<string, string> globalProperties)
    {
        IXrAImageTo3d imageTo3d = XrAiFactory.LoadImageTo3d(model, globalProperties);
        _task = imageTo3d.Execute(GetFrame().GetTexture(), GetWorkflowProperties(model, XrAiModelManager.WORKFLOW_IMAGE_TO_3D));
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

            // Make GameObject grabbable with Meta SDK
            SetupGrabbableObject(gameObject);
        }
        else
        {
            throw new Exception("Failed to load 3D model from data.");
        }
    }

    private void SetupGrabbableObject(GameObject obj)
    {
        // Add Rigidbody if not present
        if (obj.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = obj.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.useGravity = false;
            rb.isKinematic = false; // Must be false for grabbing to work
        }

        // Add collider if not present
        if (obj.GetComponent<Collider>() == null)
        {
            MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
            meshCollider.convex = true;
        }

        // Add Grabbable component from Oculus Interaction
        if (obj.GetComponent<Grabbable>() == null)
        {
            obj.AddComponent<Grabbable>();
        }
        
        // Add GrabInteractable for hand tracking
        if (obj.GetComponent<GrabInteractable>() == null)
        {
            obj.AddComponent<GrabInteractable>();
        }

        // Ensure object is on the correct layer for interaction
        obj.layer = LayerMask.NameToLayer("Default");
    }

}
