using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilitys : MonoBehaviour
{
    public static Coord[] ShuffleCoords(Coord[] dataArray)
    {
        for (int i = 0; i < dataArray.Length ; i++)
        {
            int randomIndex = Random.Range(i,dataArray.Length);

            Coord temp = dataArray[randomIndex];
            dataArray[randomIndex] = dataArray[i];
            dataArray[i] = temp;
        }

        return dataArray;
    }
}
