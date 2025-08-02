using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtificialObjects : Environments
{
    [SerializeField] private GameObject simpleObjectPrefab;
    [SerializeField] private GameObject outerObjectPrefab;
    [SerializeField] private List<GameObject> meshes = new List<GameObject>();
    [SerializeField] private List<Material> materials = new List<Material>();
    [SerializeField] private float objectScale;
    [SerializeField] private int numberOfObjects;
    [SerializeField] private float[] heights;
    [SerializeField] private float[] radii;
    [SerializeField] private float[] angles;
    private List<GameObject> meshObjects = new List<GameObject>();
    private float[] outsideRadii = new float[] { 0, 1 };

    public override void SetupEnvironment()
    {
        base.SetupEnvironment();
        SetupArtificialEnvironment(numberOfObjects, heights, radii, angles);
    }

    private void SetupArtificialEnvironment(int numberOfObjects, float[] heights, float[] radii, float[] angles)
    {
        SetOuterCylinder();

        bool searchObjectPlaced = false;
        float minHeight = heights[0];
        float maxHeight = heights[1];
        float minRadius = radii[0];
        float maxRadius = radii[1];
        float minAngle = angles[0];
        float maxAngle = angles[1];
        int maxAttempts = 10;

        for (int i = 0; i < numberOfObjects; i++)
        {
            bool positionFound = false;
            int attempts = 0;
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            while (!positionFound && attempts < maxAttempts)
            {
                attempts++;
                float height = Random.Range(minHeight, maxHeight);
                float radius = Random.Range(minRadius, maxRadius);
                float angle = Random.Range(minAngle, maxAngle);
                float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
                float z = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
                position = new Vector3(x, height, z);
                rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
                if (PositionValid(position))
                {
                    positionFound = true;
                }
            }
            // place object outside of radius so they dont overlap with other objects
            if (!positionFound)
            {
                float height = Random.Range(minHeight, maxHeight);
                float angle = Random.Range(minAngle, maxAngle);
                float radiusOutside = maxRadius + Random.Range(outsideRadii[0], outsideRadii[1]);
                float x = radiusOutside * Mathf.Cos(angle * Mathf.Deg2Rad);
                float z = radiusOutside * Mathf.Sin(angle * Mathf.Deg2Rad);
                position = new Vector3(x, height, z);
                rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
            }
            GameObject newObject;
            if (searchObjectPlaced)
            {
                newObject = Instantiate(simpleObjectPrefab, transform);
                newObject.transform.position = position;
                newObject.transform.rotation = rotation;
                newObject.transform.localScale = new Vector3(newObject.transform.localScale.x * objectScale, newObject.transform.localScale.y * objectScale, newObject.transform.localScale.z * objectScale);
                SimpleObject newObjectScript = newObject.AddComponent<SimpleObject>();
                newObjectScript.SetMeshObject(meshes[Random.Range(0, meshes.Count)]);
                newObjectScript.SetMaterial(materials[Random.Range(0, materials.Count)]);
                meshObjects.Add(newObject);
            }
            else
            {
                newObject = Instantiate(searchObjectPrefab, transform);
                newObject.transform.position = position;
                newObject.transform.rotation = rotation;
                newObject.transform.localScale = new Vector3(newObject.transform.localScale.x * objectScale, newObject.transform.localScale.y * objectScale, newObject.transform.localScale.z * objectScale);
                SearchObject newObjectScript = newObject.GetComponent<SearchObject>();
                Material matToAssign = materials[Random.Range(0, materials.Count)];
                newObjectScript.SetMaterial(matToAssign);
                meshObjects.Add(newObject);
                searchObject = newObject.transform;
                searchObjectPlaced = true;
            }
        }
    }

    private bool PositionValid(Vector3 position)
    {
        float objectRadius = objectScale;
        foreach (GameObject obj in meshObjects)
        {
            float distance = Vector3.Distance(position, obj.transform.position);
            if (distance < objectRadius * 2)
            {
                return false;
            }
        }
        return true;
    }

    private void SetOuterCylinder()
    {
        GameObject outerObject = Instantiate(outerObjectPrefab, transform);
        float outerOffset = 20;
        float outerRadius = radii[1] + outsideRadii[1] + outerOffset;
        float outerHeight = heights[1] + outerOffset;
        outerObject.transform.localScale = new Vector3(outerObject.transform.localScale.x * outerRadius, outerObject.transform.localScale.y * outerHeight, outerObject.transform.localScale.z * outerRadius);
    }
}