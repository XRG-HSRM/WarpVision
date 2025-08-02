using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixationCrossScene : Environments
{
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material inactive;
    [SerializeField] private Material active;
    [SerializeField] private List<Renderer> objectRenderers = new();
    float fixationCrossTimer = 1.5f;
    private float timer = 0.0f;
    private bool isExecutingFunction = false;
    private float switchTimer = 0.0f;
    private bool defaultMaterialActive = true;
    private bool lookingAtFixationCross = false;

    private void Update()
    {
        if (controller == null)
        {
            return;
        }
        if (controller.currentEyeTrackingScript.lookingAtSearchObject)
        {
            // eyeTracking
            timer += Time.deltaTime;
            if (timer >= fixationCrossTimer && !isExecutingFunction)
            {
                GoToNextScene();
                isExecutingFunction = true;
            }

            // material
            switchTimer = 0.0f;
            if (!defaultMaterialActive)
            {
                SetMaterial(defaultMaterial);
                defaultMaterialActive = true;
            }
            if (!lookingAtFixationCross)
            {
                SetMaterial(active);
            }

            lookingAtFixationCross = true;
        }
        else
        {
            // eyeTracking
            timer = 0.0f;
            isExecutingFunction = false;

            // material
            if (lookingAtFixationCross)
            {
                SetMaterial(inactive);
            }

            switchTimer += Time.deltaTime;
            if (switchTimer >= 0.5f)
            {
                if (defaultMaterialActive)
                {
                    SetMaterial(inactive);
                }
                else
                {
                    SetMaterial(defaultMaterial);
                }
                defaultMaterialActive = !defaultMaterialActive;
                switchTimer = 0.0f;
            }

            lookingAtFixationCross = false;
        }
    }

    private void SetMaterial(Material material)
    {
        foreach(Renderer renderer in objectRenderers)
        {
            renderer.material = material;
        }
    }

    private void GoToNextScene()
    {
        controller.fadeController.FadeOutNoDelay();
        controller.ResumeScene();
    }
}
