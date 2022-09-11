using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class mapGen : MonoBehaviour
{
    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int randomFillPercent;
    public int smoothAmount;
    public int neighborAmount;

    public GameObject wall;
    public GameObject spawner;

    public GameObject world;

    int[,] map;

    void Start()
    {
        //genMap();
    }

    public void changeSeed(string newSeed)
    {
        seed = newSeed;
    }

    void firstGen()
    {
        if (useRandomSeed)
        {
            seed = UnityEngine.Random.Range(0, 10000000000).ToString();
        }
        System.Random rand = new System.Random(seed.GetHashCode());

        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1) { map[x, y] = 1; } else
                {
                    map[x, y] = rand.Next(0, 100) < randomFillPercent ? 1 : 0;
                }
            }
        }
    }

    void placeWall(int x, int y)
    {
        int nodeCountX = map.GetLength(0);
        int nodeCountY = map.GetLength(1);
        float mapWidth = nodeCountX * 1;
        float mapHeight = nodeCountY * 1;

        Vector3 pos = new Vector3(-mapWidth / 2 + x * 1 + 1 / 2, -mapHeight / 2 + y * 1 + 1 / 2);

        GameObject placed = Instantiate(wall, pos, Quaternion.identity) as GameObject;
        placed.transform.localScale = new Vector3(UnityEngine.Random.Range(1.5f, 3f), UnityEngine.Random.Range(1.5f, 3f), 1);
    }
    void placeSpawner(int x, int y)
    {
        int nodeCountX = map.GetLength(0);
        int nodeCountY = map.GetLength(1);
        float mapWidth = nodeCountX * 1;
        float mapHeight = nodeCountY * 1;

        Vector3 pos = new Vector3(-mapWidth / 2 + x * 1 + 1 / 2, -mapHeight / 2 + y * 1 + 1 / 2);

        GameObject placed = Instantiate(spawner, pos, Quaternion.identity) as GameObject;
        placed.transform.localScale = new Vector3(UnityEngine.Random.Range(.5f, 1.5f), UnityEngine.Random.Range(.5f, 1.5f), 1);
    }

    void placeWalls()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 1 && getWalls(x, y) != 8) { placeWall(x, y); }
                if (map[x, y] == 2) { placeSpawner(x, y); }
            }
        }
    }

    public void genMap()
    {
        Debug.Log("clicked");
        map = new int[width, height];
        firstGen();
        for(int i = 0; i < smoothAmount; i++)
        {
            smooth();
        }
        placeWalls();
        //meshGen MeshGen = GetComponent<meshGen>();
        //MeshGen.GenerateMesh(map, 1);
    }

    int getWalls(int x, int y)
    {
        int wallCount = 0;
        for(int otherX = x - 1; otherX <= x + 1; otherX++)
        {
            for(int otherY = y - 1; otherY <= y + 1; otherY++)
            {
                if (otherX >= 0 && otherX < width && otherY >= 0 && otherY < height)
                {
                    if(otherX != x || otherY != y) 
                    {
                        if (map[otherX, otherY] == 1)
                        {
                            wallCount++;
                        }
                    }
                }
                else
                {
                    wallCount++;
                }
                
            }
        }
        return wallCount;
    }

    int getSpawns(int x, int y)
    {
        int wallCount = 0;
        for (int otherX = x - (width/ 12); otherX <= x + (width / 12); otherX++)
        {
            for (int otherY = y - (height / 12) ; otherY <= y + (height / 12) ; otherY++)
            {
                if (otherX >= 0 && otherX < width && otherY >= 0 && otherY < height)
                {
                    if (otherX != x || otherY != y)
                    {
                        if (map[otherX, otherY] == 2)
                        {
                            wallCount++;
                        }
                    }
                }

            }
        }
        return wallCount;
    }

    void smooth()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbors = getWalls(x, y);
                int spawners = getSpawns(x, y);
                if(neighbors > neighborAmount)
                {
                    map[x, y] = 1;
                }
                else if (neighbors <= neighborAmount && spawners == 0)
                {
                    map[x, y] = 2;
                } else if (neighbors < neighborAmount && spawners > 0)
                {
                    map[x, y] = 0;
                }
            }
        }
    }
    /*
    void OnDrawGizmos()
    {
       if(map != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (map[x, y] == 1 ? Color.black : Color.white);
                    //if (x == 0 && y == 0) { Gizmos.color = Color.green; }
                    Vector3 pos = new Vector3(-width / 2 + x, 0, -height / 2 + y);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }     
        
    }*/
}
