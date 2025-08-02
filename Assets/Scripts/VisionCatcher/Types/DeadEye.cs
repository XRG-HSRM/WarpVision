using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadEye : VisionCatcher
{
    [SerializeField] private Material originalMat;
    [SerializeField] private Material deadEyeMat;
    private Material appliedDeadEyeMat;
    private Renderer targetRenderer;
    private SearchObject searchObjectScript;

    public override void StartVisionCatcher()
    {
        searchObjectScript = transformToCatch.GetComponent<SearchObject>();
        targetRenderer = searchObjectScript.GetRenderer();
        originalMat = targetRenderer.material;
        appliedDeadEyeMat = new Material(deadEyeMat);
        appliedDeadEyeMat.CopyPropertiesFromMaterial(originalMat);
        targetRenderer.material = appliedDeadEyeMat;
        running = true;
    }

    public override void StopVisionCatcher()
    {
        if (!running) { return; }
        targetRenderer.material = originalMat;
        running = false;
    }
}
