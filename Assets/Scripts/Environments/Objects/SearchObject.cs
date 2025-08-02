using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchObject : MonoBehaviour
{
    [SerializeField] private MeshRenderer objectRenderer;

    public void SetMaterial(Material material)
    {
        if(objectRenderer != null)
        {
            objectRenderer.material = material;
        }
        else
        {
            GetComponent<MeshRenderer>().material = material;
        }
        
    }

    public Renderer GetRenderer()
    {
        return objectRenderer;
    }
}
