using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleObject : MonoBehaviour
{
    private GameObject meshObject;
    private Renderer objectRenderer;

    public void SetMeshObject(GameObject meshObject)
    {
        this.meshObject = Instantiate(meshObject, transform);
        objectRenderer = this.meshObject.GetComponent<Renderer>();
    }

    public void SetMaterial(Material material)
    {
        meshObject.GetComponent<MeshRenderer>().material = material;
    }

    public Renderer GetRenderer()
    {
        return objectRenderer;
    }
}
