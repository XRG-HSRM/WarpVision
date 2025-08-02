using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;

public class EyeTrackingRaycast : MonoBehaviour
{
    public static Vector3? CurrentGazeHitPoint { get; private set; } = null;
    private float rayLength = 100f;
    [HideInInspector]
    public Transform eyetrackingTransform;
    public bool eyeTrackingActive = false;
    [HideInInspector]
    public bool lookingAtSearchObject = false;
    [HideInInspector]
    public Transform mainCamera;
    public Ray gazeRay;
    public bool isValid = false;

    private void Update()
    {
        if (eyeTrackingActive)
        {
            CheckEyeOpenness();
            PerformEyeGazeRaycast();
        }
    }

    private void CheckEyeOpenness()
    {
        SRanipal_Eye.GetVerboseData(out var verboseData);
        if (verboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
        {
            isValid = true;
        }
        else
        {
            isValid = false;
        }
    }

    private void PerformEyeGazeRaycast()
    {
        if (eyetrackingTransform != null && SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out gazeRay))
        {
            gazeRay.origin = mainCamera.position;
            gazeRay.direction = mainCamera.TransformDirection(gazeRay.direction);

            RaycastHit hitInfo;

            if (Physics.Raycast(gazeRay.origin, gazeRay.direction, out hitInfo, rayLength, ~0, QueryTriggerInteraction.Collide))
            {
                if (hitInfo.collider.CompareTag("SearchObject"))
                {
                    lookingAtSearchObject = true;

                }
                else
                {
                    lookingAtSearchObject = false;
                }
                CurrentGazeHitPoint = hitInfo.point;
                eyetrackingTransform.position = hitInfo.point;
            }
            else
            {
                CurrentGazeHitPoint = null;
                lookingAtSearchObject = false;
                eyetrackingTransform.position = gazeRay.origin + (gazeRay.direction.normalized * 10f);
            }
        }
    }

}
