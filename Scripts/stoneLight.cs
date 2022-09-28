using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stoneLight : MonoBehaviour
{
    public UnityEngine.Rendering.Universal.Light2D light;

    Color nearestSpawnerColor()
    {
        GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawner");
        GameObject min = spawners[0];
        GameObject prevmin = spawners[1];
        bool usePrev = false;
        foreach(GameObject spawner in spawners)
        {
            if((spawner.transform.position - this.transform.position).magnitude < (min.transform.position - this.transform.position).magnitude)
            {
                prevmin = min;
                usePrev = true;
                min = spawner;
            }
        }
        if(!usePrev) { return min.GetComponent<Spawner>().light.color; }
        Color average = (min.GetComponent<Spawner>().light.color + prevmin.GetComponent<Spawner>().light.color) / 2;
        return average;
    }

    // Update is called once per frame
    void Update()
    {
        if (Random.Range(0,100) < 15)
        {
            Color average = nearestSpawnerColor();
            this.GetComponent<SpriteRenderer>().color = average;
        }
    }
}
