using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

public class RealisticSetting : Environments
{
    [SerializeField] private string UniqueIdentifier;
    [SerializeField] private GameObject coin1;
    [SerializeField] private GameObject coin2;
    [SerializeField] private Transform searchObjectLocations;
    private List<Transform> searchObjectLocationsList = new List<Transform>();
    private Transform searchObjectLocation;
    bool objectSet = false;

    private void Awake()
    {
        foreach (Transform t in searchObjectLocations)
        {
            searchObjectLocationsList.Add(t);
        }
        if (searchObjectLocationsList.Count < 10)
        {
            Debug.LogError("Not enough locations to position the searchObject!");
        }
        if (UniqueIdentifier == "")
        {
            UniqueIdentifier = transform.name;
        }
    }

    private Transform GetRelevantSearchObjectLocation()
    {
        searchObjectLocationsList = controller.utilities.ShuffleList(searchObjectLocationsList, controller.seed);
        List<string> usedLocations = controller.GetAllUsedSearchLocations();
        for (int i = 0; i < searchObjectLocationsList.Count; i++)
        {
            if (!objectSet && !usedLocations.Contains(UniqueEnvStrings(i)))
            {
                searchObjectLocation = searchObjectLocationsList[i];
                controller.SearchLocationUsed(UniqueEnvStrings(i));
                objectSet = true;
            }
            else
            {
                int random = Random.Range(0, 5);
                if (random > 2) continue;
                GameObject newObject = Instantiate(random < 1 ? coin1 : coin2, environmentTransform);
                newObject.transform.position = searchObjectLocationsList[i].transform.position;
                newObject.transform.eulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
            }
        }
        if (searchObjectLocation == null)
        {
            Debug.LogError("Error in setting search locations. Using default.");
            searchObjectLocation = searchObjectLocationsList[0];
        }

        return null;
    }

    private string UniqueEnvStrings(int i)
    {
        return i + UniqueIdentifier;
    }

    public override void SetupEnvironment()
    {
        GetRelevantSearchObjectLocation();
        GameObject newObject = Instantiate(searchObjectPrefab, environmentTransform);
        newObject.transform.position = searchObjectLocation.transform.position;
        newObject.transform.eulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
        searchObject = newObject.transform;
    }
}
