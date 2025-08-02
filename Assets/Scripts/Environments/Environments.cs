using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environments : MonoBehaviour
{
    public Transform environmentTransform;
    public GameObject searchObjectPrefab;
    public Transform searchObject;
    public string environmentName;
    public Transform questionLocation;
    public float maxSearchDistance;
    public Controller controller;

    public virtual void SetupEnvironment()
    {
        environmentName = transform.name;
    }

    public void ApplyVisionCatcherToScene(VisionCatcher visionCatcher)
    {

    }

    public virtual Transform GetSearchObject()
    {
        return searchObject;
    }

    public virtual Transform GetQuestionTransform()
    {
        return questionLocation;
    }
}
