using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class random : MonoBehaviour
{
    public string seed;
    public System.Random rand;

    void Start()
    {
        seed = UnityEngine.Random.Range(0, 100000).ToString();
        rand = new System.Random(seed.GetHashCode());
    }
    public void changeSeed(string newSeed)
    {
        seed = newSeed;
        rand = new System.Random(seed.GetHashCode());
    }

    public int range(int start, int end)
    {
        return rand.Next(start, end);
    }

    public float range(float start, float end)
    {
        return (float)(rand.NextDouble() * (end - start)) + start;
    }

    public float value()
    {
        return (float)rand.NextDouble();
    }
}
