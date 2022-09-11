using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public float radius;
    public GameObject doid;
    public GameObject food;
    public GameObject world;

    public int maxTargets;

    public List<Collider2D> targets;

    public UnityEngine.Rendering.Universal.Light2D light;

    public LayerMask walls;
    public LayerMask targetMask;

    void Start()
    {
        world = GameObject.FindWithTag("World");
        //targets = new Collider2D[world.GetComponent<World>().doidMin / GameObject.FindGameObjectsWithTag("Spawner").Length];
        maxTargets = world.GetComponent<World>().doidMin / GameObject.FindGameObjectsWithTag("Spawner").Length;
        StartCoroutine(getOut());
    }
    
    IEnumerator getOut()
    {
        this.GetComponent<CircleCollider2D>().isTrigger = false;

        yield return new WaitForSeconds(5);

        this.GetComponent<CircleCollider2D>().isTrigger = true;
        this.transform.localScale = new Vector3(radius, radius, 1);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "Doid" && targets.Count < maxTargets)
        {
            targets.Add(col);
        }
        
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.tag == "Doid")
        {
            targets.Remove(col);
        }
    }

    void Empty(Collider2D[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            list[i] = null;
        }
    }

    Color averageColor()
    {
        int num = 0;
        Vector3 colorSum = new Vector3(0, 0, 0);
        foreach(Collider2D doid in targets)
        {
            num++;
            Vector4 otherColor4 = doid.gameObject.GetComponent<Doid>().light.color;
            Vector3 otherColor = otherColor4;
            colorSum += otherColor;
        }
        Vector3 vecColor = (new Vector3(colorSum.x / num, colorSum.y / num, colorSum.z / num)).normalized;
        return new Color(vecColor.x, vecColor.y, vecColor.z, .15f);
    }

    int numTargets()
    {
        int sum = 0;
        for(int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
                sum++;
        }
        return sum;
    }

    float hashNum(float num)
    {
        var hash = Hash128.Compute(num);
        float prod = 1;
        foreach (char c in hash.ToString())
        {
            prod *= (float)c / 62;
        }
        return prod;
    }

    public Color color(int species, float age, float ageMax)
    {
        Vector3 col = new Vector3(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        Vector4 newCol = (new Vector4(hashNum(species) + col.x, hashNum(species + 1) + col.y, hashNum(species + 2) + col.z, age / ageMax)).normalized * 100;
        return new Color(newCol.x, newCol.y, newCol.z, newCol.w);
    }

    public bool canSee(Vector2 vector)
    {
        Vector2 Pos = this.transform.position;
        RaycastHit2D wall = Physics2D.Raycast(Pos, vector.normalized, vector.magnitude, walls);
        //Debug.DrawLine(Pos, Pos + vector, Color.white);
        //Debug.Log(wall.collider == null);
        return wall.collider == null;
    }

    void spawnDoid(int species)
    {
        Vector2 randDir = (new Vector2(Random.Range(-100, 100), Random.Range(-100, 100))).normalized;
        Vector2 randVec = randDir * radius * Random.Range(0f, 1f);
        Vector2 pos = this.transform.position;
        Vector3 randPos = pos + randVec;
        if (canSee(randVec))
        {
            GameObject dude = Instantiate(doid, randPos, Quaternion.identity) as GameObject;

            dude.GetComponent<Brain>().randomAdjBrain();
            dude.GetComponent<Brain>().mutationRate = Random.Range(0, world.GetComponent<World>().mutationRateMax);
            dude.GetComponent<Brain>().deletionChance = Random.Range(0, world.GetComponent<World>().deletionChanceMax);
            dude.GetComponent<Brain>().connectionChance = Random.Range(0, world.GetComponent<World>().connectionChanceMax);
            dude.GetComponent<Doid>().light.color = color(species, 0, dude.GetComponent<Doid>().ageMax);
            dude.GetComponent<Doid>().species = species.ToString();
            dude.GetComponent<Doid>().radius = Random.Range(dude.GetComponent<Doid>().radiusMin, dude.GetComponent<Doid>().radiusMax);
            dude.GetComponent<Doid>().birthCost = Random.Range(dude.GetComponent<Doid>().birthMin, dude.GetComponent<Doid>().energyMax);
            dude.GetComponent<Doid>().forceMax = Random.Range(.01f, world.GetComponent<World>().forceMax);
            float height = Random.Range(world.GetComponent<World>().heightMin, world.GetComponent<World>().heightMax);
            float width = Random.Range(world.GetComponent<World>().widthMin, world.GetComponent<World>().widthMax);
            dude.GetComponent<Doid>().height = height;
            dude.GetComponent<Doid>().width = width;
            float offset = (world.GetComponent<World>().heightMax * world.GetComponent<World>().widthMax) / 4;
            dude.GetComponent<Doid>().forceMax = world.GetComponent<World>().forceMax* (1 - ((height* width) / ((world.GetComponent<World>().heightMax * world.GetComponent<World>().widthMax) + offset)));
            dude.GetComponent<Doid>().doid.localScale = new Vector3(dude.GetComponent<Doid>().height / 3, dude.GetComponent<Doid>().width / 3, 1);
        }
            
    }

    void spawnFood()
    {
        Vector2 randDir = (new Vector2(Random.Range(-100, 100), Random.Range(-100, 100))).normalized;
        Vector2 randVec = randDir * radius * Random.Range(0f, 1f);
        Vector2 pos = this.transform.position;
        Vector3 randPos = pos + randVec;
        if (canSee(randVec))
        {
            Instantiate(food, randPos, Quaternion.identity);
        }
    }

    void FixedUpdate()
    {
        if(Random.Range(0f, 100f) < 2.5f)
        {
            light.color = averageColor();
            if (targets.Count < maxTargets && GameObject.FindGameObjectsWithTag("Doid").Length < world.GetComponent<World>().doidMin)
            {
                spawnDoid(world.GetComponent<World>().speciesCounter);
                world.GetComponent<World>().speciesCounter++;
            }
        }
        if (Random.Range(0, 100) < 40 && GameObject.FindGameObjectsWithTag("Food").Length < world.GetComponent<World>().foodMin)
        {
            spawnFood();
        }
    }
}
