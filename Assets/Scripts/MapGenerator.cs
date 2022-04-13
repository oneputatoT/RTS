using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] GameObject tilePrefab;
    [SerializeField] Vector2 mapSize;
    [SerializeField] Transform mapHolder;
    [SerializeField][Range(0,1)] float outlinePercent;
    [SerializeField] GameObject obsPrefab;
    //[SerializeField] float obsCount;

    [SerializeField] List<Coord> allTilesCoord;
    Queue<Coord> shuffledQueue;
    [SerializeField] Color frontColor;
    [SerializeField] Color backColor;

    [SerializeField] float minHight;
    [SerializeField] float maxHight;

    [SerializeField] [Range(0f, 1f)] float obsPercent;

    [Header("NavMesh")]
    [SerializeField] Vector2 maxMapSize;
    [SerializeField] GameObject navMeshObs;

    [Header("Player")]
    [SerializeField] GameObject player;

    Coord mapCenter;     //中心位置
    bool[,] mapObstacles;    //该位置是否有障碍物

    private void Start()
    {
        Generator();
        Initialize();
    }

    private void Initialize()
    {
        Instantiate(player, new Vector3(0f, 1f, 0f), Quaternion.identity);
    }

    private void Generator()
    {
        allTilesCoord = new List<Coord>();

        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                Vector3 newPos = new Vector3(-mapSize.x / 2 + 0.5f+i, 0f, -mapSize.y / 2 + 0.5f+j);
                GameObject spawnTile = Instantiate(tilePrefab, newPos, Quaternion.Euler(90, 0, 0), mapHolder);
                spawnTile.transform.localScale *= (1 - outlinePercent);

                allTilesCoord.Add(new Coord(i, j));
            }
        }

        //for (int i = 0; i < obsCount; i++)
        //{
        //    Coord randomCoord = allTilesCoord[UnityEngine.Random.Range(0,allTilesCoord.Count)];
        //    Vector3 newpos = new Vector3(-mapSize.x / 2 + 0.5f + randomCoord.x, 0.5f, -mapSize.y / 2 + 0.5f + randomCoord.y);
        //    GameObject spawnObs = Instantiate(obsPrefab, newpos, Quaternion.identity, mapHolder);
        //    spawnObs.transform.localScale *= (1 - outlinePercent);
        //}

        shuffledQueue = new Queue<Coord>(Utilitys.ShuffleCoords(allTilesCoord.ToArray()));

        int obsCount = (int)(mapSize.x * mapSize.y * obsPercent);
        mapCenter = new Coord((int)mapSize.x / 2, (int)mapSize.y / 2);
        mapObstacles = new bool[(int)mapSize.x, (int)mapSize.y];       //初始话bool容器

        int currentObsCount = 0;

        for (int i = 0; i < obsCount; i++)
        {
            Coord randomCoord = GetRandomCoord();

            mapObstacles[randomCoord.x, randomCoord.y] = true;   //首先认为可以有障碍物
            currentObsCount++;    //当前障碍物增加

            if (randomCoord != mapCenter && MapIsFullyAccesible(mapObstacles, currentObsCount))
            {
                float hight = Mathf.Lerp(minHight, maxHight, UnityEngine.Random.Range(0f, 1f));
                Vector3 newpos = new Vector3(-mapSize.x / 2 + 0.5f + randomCoord.x, hight / 2.0f, -mapSize.y / 2 + 0.5f + randomCoord.y);
                GameObject spawnObs = Instantiate(obsPrefab, newpos, Quaternion.identity, mapHolder);
                spawnObs.transform.localScale = new Vector3(1 - outlinePercent, hight, 1 - outlinePercent);

                Material material = spawnObs.GetComponent<Renderer>().material;
                float t = randomCoord.y / mapSize.y;
                material.color = Color.Lerp(frontColor, backColor, t);

            }
            else
            {
                mapObstacles[randomCoord.x, randomCoord.y] = false;
                currentObsCount--;
            }
        }

        GameObject obsForward = Instantiate(navMeshObs, new Vector3(0f, 0f, (mapSize.y + maxMapSize.y) / 4), Quaternion.identity, mapHolder);
        obsForward.transform.localScale = new Vector3(mapSize.x, 5f, (maxMapSize.y - mapSize.y) / 2);

        GameObject obsBack = Instantiate(navMeshObs, new Vector3(0f, 0f, -(mapSize.y + maxMapSize.y) / 4), Quaternion.identity, mapHolder);
        obsBack.transform.localScale = new Vector3(mapSize.x, 5f, (maxMapSize.y - mapSize.y) / 2);

        GameObject obsLeft = Instantiate(navMeshObs, new Vector3(-(mapSize.x + maxMapSize.x) / 4, 0f, 0f), Quaternion.identity, mapHolder);
        obsLeft.transform.localScale = new Vector3((maxMapSize.x - mapSize.x) / 2, 5f, mapSize.y);

        GameObject obsRight = Instantiate(navMeshObs, new Vector3((mapSize.x + maxMapSize.x) / 4, 0f, 0f), Quaternion.identity, mapHolder);
        obsRight.transform.localScale = new Vector3((maxMapSize.x - mapSize.x) / 2, 5f, mapSize.y);

    }

    private bool MapIsFullyAccesible(bool[,] mapObstacles,int currentObsCount)
    {
        bool[,] mapFlags = new bool[mapObstacles.GetLength(0), mapObstacles.GetLength(1)];

        Queue<Coord> queue = new Queue<Coord>();

        queue.Enqueue(mapCenter);
        mapFlags[mapCenter.x, mapCenter.y]=true;    //中心点可以行走且无障碍物

        int accessibleCount = 1;     //可行走瓦片数量

        while (queue.Count > 0)
        {
            Coord currentTile = queue.Dequeue();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighborX = currentTile.x + x;
                    int neighborY = currentTile.y + y;

                    if (x == 0 || y == 0)
                    {
                        if (neighborX >= 0 && neighborX < mapObstacles.GetLength(0)
                            && neighborY >= 0 && neighborY < mapObstacles.GetLength(1))
                        {
                            if (!mapFlags[neighborX, neighborY] && !mapObstacles[neighborX, neighborY])
                            {
                                mapFlags[neighborX, neighborY] = true;
                                accessibleCount++;
                                queue.Enqueue(new Coord(neighborX, neighborY));
                            }
                        }
                    }
                }
            }
        }

        int totalAccessible = (int)(mapSize.x * mapSize.y - currentObsCount);
        return totalAccessible==accessibleCount;
    }

    private Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledQueue.Dequeue();
        shuffledQueue.Enqueue(randomCoord);
        return randomCoord;
    }
}

[Serializable]
public struct Coord
{
    public int x;
    public int y;

    public Coord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static bool operator ==(Coord a,Coord b)
    {
        return (a.x == b.x) && (a.y == b.y);
    }

    public static bool operator !=(Coord a, Coord b)
    {
        return !(a == b);
    }
}
