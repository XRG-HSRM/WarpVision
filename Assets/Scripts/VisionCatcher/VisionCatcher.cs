using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionCatcher : MonoBehaviour
{
    public string visionCatcherName;
    public Transform transformToCatch;
    public Camera mainCamera;
    public bool running;
    [HideInInspector]
    public Controller controller;

    private void Awake()
    {
        running = false;
    }

    public void SetupVisionCatcher(Transform transformToCatch, Camera mainCamera, Controller controller)
    {
        this.transformToCatch = transformToCatch;
        this.mainCamera = mainCamera;
        this.controller = controller;
    }

    public virtual void StartVisionCatcher()
    {

    }

    public virtual void StopVisionCatcher()
    {

    }
}
