using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRCamera : MonoBehaviour
{
    public Transform parent;
    public delegate void RenderImageDelegate(RenderTexture source, RenderTexture destination);
    public RenderImageDelegate customRenderImageFunction;

    private void Awake()
    {
        DontDestroyOnLoad(parent);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (customRenderImageFunction != null)
        {
            customRenderImageFunction(source, destination);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    private void DefaultRenderFunction(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest);
    }

    public void SetCustomRenderFunction(RenderImageDelegate newRenderFunction)
    {
        customRenderImageFunction = newRenderFunction;
    }

    public void ResetToDefaultRenderFunction()
    {
        customRenderImageFunction = DefaultRenderFunction;
    }
}
