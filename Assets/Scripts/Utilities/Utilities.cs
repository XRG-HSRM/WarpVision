using System.Collections.Generic;
using UnityEngine;

public class Utilities : MonoBehaviour
{
    public List<T> ShuffleList<T>(List<T> list, int seed)
    {
        System.Random rng = new System.Random(seed);
        List<T> shuffledList = new List<T>(list);
        int n = shuffledList.Count;
        for (int i = 0; i < n; i++)
        {
            int randomIndex = rng.Next(i, n);
            T temp = shuffledList[randomIndex];
            shuffledList[randomIndex] = shuffledList[i];
            shuffledList[i] = temp;
        }
        return shuffledList;
    }
}
