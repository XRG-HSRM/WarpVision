using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/*
WarpVision
*/

public class TunnelVision : VisionCatcher
{
    [SerializeField] private WVType type;
    public Material pinchMaterial;
    [SerializeField] private float pinchStrength = 0.5f;
    [SerializeField] private float basePinchRadius = 0.5f;
    [SerializeField] private float growth = 10f;
    //[SerializeField] private float smoothness = 0.1f;
    [SerializeField] private bool hasDelayBeforeIncrease = false;
    [SerializeField] private float delayBeforeIncrease = 2f;
    [SerializeField] private float expBase = 20f;
    [SerializeField] float maxIncreasePerFrame = 0.05f;
    [SerializeField] float maxDecreasePerFrame = 0.05f;
    VRCamera cameraScript;

    private float timeSinceNotLookedAt = 0f;
    float lastValidPinchRadius;
    float currentPinchRadius;

    public override void StartVisionCatcher()
    {
        if (running) { return; }
        cameraScript = mainCamera.GetComponent<VRCamera>();
        cameraScript.SetCustomRenderFunction(WarpVisionRenderer);
        running = true;
        currentPinchRadius = basePinchRadius;
        lastValidPinchRadius = currentPinchRadius;

    }

    public override void StopVisionCatcher()
    {
        if (!running) { return; }
        cameraScript.ResetToDefaultRenderFunction();
        running = false;
    }


    private void Update()
    {
        if (!running
            ) { return; }
        if (IsObjectBehindCamera())
        {
            pinchMaterial.SetFloat("_PinchRadius", 0f);
            return;
        }
        switch (type)
        {
            case (WVType.NoChange):
                break;
            case (WVType.LogarithmicIncrease):
                LogarithmicIncrease();
                break;
            case (WVType.ExponentialIncrease):
                ExponentialIncrease();
                break;
            default: break;
        }
    }

    public bool IsObjectBehindCamera()
    {
        Vector3 directionToObject = transformToCatch.position - mainCamera.transform.position;
        Vector3 cameraForward = mainCamera.transform.forward;
        float dotProduct = Vector3.Dot(cameraForward, directionToObject.normalized);
        return dotProduct < 0;
    }

    private void LogarithmicIncrease()
    {
        float distance = Mathf.Abs(lastValidPinchRadius - currentPinchRadius);
        float logChange = Mathf.Log(1f + distance, expBase);
        float maxLogIncrease = maxIncreasePerFrame * logChange;
        float maxLogDecrease = maxDecreasePerFrame * logChange;
        if (controller.currentEyeTrackingScript.isValid)
        {
            Vector3 origin = controller.currentEyeTrackingScript.gazeRay.origin;
            Vector3 direction = controller.currentEyeTrackingScript.gazeRay.direction;
            Vector3 vectorToTarget = transformToCatch.position - origin;
            float angle = Vector3.Angle(direction, vectorToTarget);
            float angleThreshold = 5f;
            float adjustedAngle = angle - angleThreshold;
            float effectiveAngle = adjustedAngle < 0f ? 0 : adjustedAngle;
            float angleNormalized = Mathf.Clamp01(effectiveAngle / (90f - angleThreshold));
            float logScaleFactor = Mathf.Log10(1f + angleNormalized * growth);
            float targetPinchRadius = basePinchRadius * logScaleFactor;
            lastValidPinchRadius = targetPinchRadius;
            if (!controller.currentEyeTrackingScript.lookingAtSearchObject)
            {
                timeSinceNotLookedAt += Time.deltaTime;
                if (
                    !hasDelayBeforeIncrease || timeSinceNotLookedAt >= delayBeforeIncrease)
                {
                    if (lastValidPinchRadius < currentPinchRadius)
                    {
                        currentPinchRadius = Mathf.MoveTowards(currentPinchRadius, lastValidPinchRadius, maxLogDecrease);
                    }
                    else
                    {
                        currentPinchRadius = Mathf.MoveTowards(currentPinchRadius, lastValidPinchRadius, maxLogIncrease);
                    }
                }
            }
            else
            {
                timeSinceNotLookedAt = 0f;
                //currentPinchRadius = Mathf.Lerp(currentPinchRadius, 0f, smoothness);
                currentPinchRadius = Mathf.MoveTowards(currentPinchRadius, 0, maxLogDecrease);
            }
        }
        else
        {
            if (lastValidPinchRadius < currentPinchRadius)
            {
                currentPinchRadius = Mathf.MoveTowards(currentPinchRadius, lastValidPinchRadius, maxLogDecrease);
            }
            else
            {
                currentPinchRadius = Mathf.MoveTowards(currentPinchRadius, lastValidPinchRadius, maxLogIncrease);
            }
        }
        pinchMaterial.SetFloat("_PinchRadius", currentPinchRadius);
    }

    private void ExponentialIncrease()
    {
        float distance = Mathf.Abs(lastValidPinchRadius - currentPinchRadius);
        float expChange = Mathf.Pow(2f, distance) - 1f;
        float cappedIncreaseChange = Mathf.Min(expChange, maxIncreasePerFrame);
        float cappedDecreaseChange = Mathf.Min(expChange, maxDecreasePerFrame);
        if (controller.currentEyeTrackingScript.isValid)
        {
            Vector3 origin = controller.currentEyeTrackingScript.gazeRay.origin;
            Vector3 direction = controller.currentEyeTrackingScript.gazeRay.direction;
            Vector3 vectorToTarget = transformToCatch.position - origin;
            float angle = Vector3.Angle(direction, vectorToTarget);
            float angleThreshold = 5f;
            float adjustedAngle = angle - angleThreshold;
            float effectiveAngle = adjustedAngle < 0f ? 0 : adjustedAngle;
            float angleNormalized = Mathf.Clamp01(effectiveAngle / (90f - angleThreshold));
            float expScaleFactor = Mathf.Pow(2f, angleNormalized * growth) - 1f;
            float targetPinchRadius = basePinchRadius * expScaleFactor;
            lastValidPinchRadius = targetPinchRadius;
            if (!controller.currentEyeTrackingScript.lookingAtSearchObject)
            {
                timeSinceNotLookedAt += Time.deltaTime;

                if (!hasDelayBeforeIncrease || timeSinceNotLookedAt >= delayBeforeIncrease)
                {
                    if (lastValidPinchRadius < currentPinchRadius)
                    {
                        currentPinchRadius = Mathf.MoveTowards(currentPinchRadius, lastValidPinchRadius, cappedDecreaseChange);
                    }
                    else
                    {
                        currentPinchRadius = Mathf.MoveTowards(currentPinchRadius, lastValidPinchRadius, cappedIncreaseChange);
                    }
                }
                else
                {
                    currentPinchRadius = Mathf.MoveTowards(currentPinchRadius, 0f, cappedDecreaseChange);
                }
            }
            else
            {
                timeSinceNotLookedAt = 0f;
                currentPinchRadius = Mathf.MoveTowards(currentPinchRadius, 0f, cappedDecreaseChange);
            }
        }
        else
        {
            if (lastValidPinchRadius < currentPinchRadius)
            {
                currentPinchRadius = Mathf.MoveTowards(currentPinchRadius, lastValidPinchRadius, cappedDecreaseChange);
            }
            else
            {
                currentPinchRadius = Mathf.MoveTowards(currentPinchRadius, lastValidPinchRadius, cappedIncreaseChange);
            }
        }
        pinchMaterial.SetFloat("_PinchRadius", currentPinchRadius);
    }



    private void WarpVisionRenderer(RenderTexture source, RenderTexture destination)
    {
        if (!running) { return; }
        Vector3 screenPos = mainCamera.WorldToViewportPoint(transformToCatch.position);
        pinchMaterial.SetFloat("_PinchStrength", pinchStrength);
        pinchMaterial.SetFloat("_PinchRadius", pinchMaterial.GetFloat("_PinchRadius"));
        pinchMaterial.SetVector("_PinchCenter", new Vector4(screenPos.x, screenPos.y, 0, 0));
        pinchMaterial.SetFloat("_AspectRatio", 1f);
        RenderTexture.active = destination;
        GL.PushMatrix();
        GL.LoadOrtho();
        Graphics.Blit(source, destination, pinchMaterial);
        GL.PopMatrix();
    }
}

public enum WVType
{
    NoChange,
    DistanceBased,
    LinearIncrease,
    LogarithmicIncrease,
    Constant,
    ExponentialIncrease
}
