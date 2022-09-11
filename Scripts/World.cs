using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class World : MonoBehaviour
{
    public int doidMin = 100;
    public int doidMax = 500;
    public int foodMin = 100;
    public int foodMax = 150;
    public float heightMin = .03f;
    public float heightMax = .15f;
    public float widthMin = .03f;
    public float widthMax = .15f;
    public float forceMax = 3;
    public int speciesCounter = 0;
    public LayerMask doids;
    public bool follow;
    public bool looking = true;
    public GameObject selected;
    public Text Stats;
    public Text worldStats;
    public bool stats;
    public List<(float, float, float)> doidZones = new List<(float, float, float)>();
    public List<(float, float, float)> foodZones = new List<(float, float, float)>();

    public float year = 0;

    public Camera mainCamera;
    public GameObject doid;
    public GameObject food;
    public GameObject canvas;
    public brainVisualization vis;

    public float mutationRateMax;
    public int connectionChanceMax;
    public int deletionChanceMax;

    float y;
    float x;

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

    void spawnDoid(int species)
    {
        //Debug.Log(forceMax);
        int zone = Random.Range(0, doidZones.Count);
        Vector2 randDir = (new Vector2(Random.Range(-100, 100), Random.Range(-100, 100))).normalized;
        Vector2 randVec = randDir * doidZones[zone].Item3 * Random.Range(0f, 1f);
        Vector3 randPos = (new Vector2(doidZones[zone].Item1, doidZones[zone].Item2)) + randVec;
        GameObject dude = Instantiate(doid, randPos, Quaternion.identity) as GameObject;
        dude.GetComponent<Brain>().randomAdjBrain();
        dude.GetComponent<Brain>().mutationRate = Random.Range(0, mutationRateMax);
        dude.GetComponent<Brain>().deletionChance = Random.Range(0, deletionChanceMax);
        dude.GetComponent<Brain>().connectionChance = Random.Range(0, connectionChanceMax);
        dude.GetComponent<Doid>().light.color = color(species, 0, dude.GetComponent<Doid>().ageMax);
        dude.GetComponent<Doid>().species = species.ToString();
        dude.GetComponent<Doid>().radius = Random.Range(dude.GetComponent<Doid>().radiusMin, dude.GetComponent<Doid>().radiusMax);
        dude.GetComponent<Doid>().birthCost = Random.Range(dude.GetComponent<Doid>().birthMin, dude.GetComponent<Doid>().energyMax);
        dude.GetComponent<Doid>().forceMax = Random.Range(.01f, forceMax);
        float height = Random.Range(heightMin, heightMax);
        float width = Random.Range(widthMin, widthMax);
        dude.GetComponent<Doid>().height = height;
        dude.GetComponent<Doid>().width = width;
        float offset = (heightMax * widthMax) / 2;
        dude.GetComponent<Doid>().forceMax = forceMax * (1 - ((height * width) / ((heightMax * widthMax) + offset)));
        dude.GetComponent<Doid>().doid.localScale = new Vector3(dude.GetComponent<Doid>().height, dude.GetComponent<Doid>().width, 1);
        //TODO: MAKE WIDTH AND HEIGHT SEPERATE VARIABLES AND MAKE MOVE ENERGY BE A FUNCTION OF FORCE AND VOLUME
    }

    void spawnFood()
    {
        int zone = Random.Range(0, foodZones.Count);
        Vector2 randDir = (new Vector2(Random.Range(-100, 100), Random.Range(-100, 100))).normalized;
        Vector2 randVec = randDir * foodZones[zone].Item3 * Random.Range(0f, 1f);
        Vector3 randPos = (new Vector2(foodZones[zone].Item1, foodZones[zone].Item2)) + randVec;
        Instantiate(food, randPos, Quaternion.identity);
    }

    void Start()
    {
        year = 0;
        speciesCounter = 0;
        y = mainCamera.orthographicSize * 2;
        x = mainCamera.orthographicSize * 2 * mainCamera.aspect;
        //(float, float, float) foodSpawn = (0, 0, y/2);
        //doidZones.Add(foodSpawn);
        //foodZones.Add(foodSpawn);
        /*(for (int i = 0; i < doidMin; i++)
        {
            spawnDoid(i);
            speciesCounter++;
        }
        for (int i = 0; i < foodMin; i++)
        {
            spawnFood();
        }*/
    }

    GameObject touchDoid()
    {
        if (Input.GetKeyDown("mouse 2"))
        {
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = -10;
            Collider2D hit = Physics2D.OverlapCircle(mousePosition, .15f, doids);
            if (hit != null)
            {
                //Debug.Log(hit);
                stats = true;
                GameObject dude = hit.gameObject;
                vis.brain = dude.GetComponent<Brain>();
                vis.doid = dude.GetComponent<Doid>();
                vis.visualize();
                return dude;

            }
        }
        return selected;
    }

    void updateWorldStats()
    {
        worldStats.text = "Population: " + GameObject.FindGameObjectsWithTag("Doid").Length.ToString() +
                        "\nFood: " + GameObject.FindGameObjectsWithTag("Food").Length.ToString() +
                        "\nYear: " + year.ToString();
    }

    void followDoid(GameObject dude)
    {
        if (stats)
        {
            if (follow)
            {
                Vector3 cameraPos = new Vector3(dude.GetComponent<Transform>().position.x, dude.GetComponent<Transform>().position.y, -2);
                mainCamera.transform.position = cameraPos;
                float y = mainCamera.orthographicSize * 2;
                Vector2 canvasSize = new Vector2(y * mainCamera.aspect, y);
                canvas.GetComponent<RectTransform>().position = cameraPos;
                transform.LookAt(dude.GetComponent<Transform>());
            }

            Stats.enabled = true;
            Stats.text = "Species: " + dude.GetComponent<Doid>().species.ToString() +
                "\nGen: " + dude.GetComponent<Doid>().generation.ToString() +
                "\nFood Eaten: " + dude.GetComponent<Doid>().foodEaten.ToString() +
                "\nage: " + dude.GetComponent<Doid>().age.ToString() +
                "\nhealth: " + dude.GetComponent<Doid>().health +
                "\nheight: " + dude.GetComponent<Doid>().height +
                "\nwidth: " + dude.GetComponent<Doid>().width +
                "\nenergy: " + dude.GetComponent<Doid>().energy +
                "\nbirthCost: " + dude.GetComponent<Doid>().birthCost +
                "\nradius: " + dude.GetComponent<Doid>().radius +
                "\nforceMax: " + dude.GetComponent<Doid>().forceMax +
                "\nconnections: " + dude.GetComponent<Brain>().adjConnections.edgesCount +
                "\n\ninternalNodeOne: " + dude.GetComponent<Doid>().middle[0] +
                "\ninternalNodeTwo: " + dude.GetComponent<Doid>().middle[1] +
                "\ninternalNodeThree: " + dude.GetComponent<Doid>().middle[2] +
                "\ninternalNodeFour: " + dude.GetComponent<Doid>().middle[3] +
                "\ninternalNodeFive: " + dude.GetComponent<Doid>().middle[4] +
                "\ninternalNodeSix: " + dude.GetComponent<Doid>().middle[5] +
                "\n\nsound: " + dude.GetComponent<Doid>().inputs[24] +
                "\nweight: " + dude.GetComponent<Doid>().inputs[25] +
                "\n\nmagnitude: " + dude.GetComponent<Doid>().outputs[0] +
                "\nangle: " + dude.GetComponent<Doid>().outputs[1] +
                "\nasex: " + dude.GetComponent<Doid>().outputs[2] +
                "\nsignal: " + dude.GetComponent<Doid>().outputs[4] +
                "\n\nmemCell1: " + dude.GetComponent<Doid>().inputs[3 * dude.GetComponent<Doid>().numRays + 2] +
                "\nmemCell2: " + dude.GetComponent<Doid>().inputs[3 * dude.GetComponent<Doid>().numRays + 6]; 


        }
    }

    void FixedUpdate()
    {
        year += .001f;
    }

    void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            Debug.Log("esc");
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
        updateWorldStats();
        float scroll = -Input.mouseScrollDelta.y * 1;
        if (Camera.main.orthographicSize + scroll > 0) { Camera.main.orthographicSize += scroll; }
        selected = touchDoid();
        if (Input.GetKeyDown("f")) { follow = true;}
        if (Input.GetKeyDown("space") || selected == null) {
            vis.unvisualize();
            follow = false;
            stats = false;
            Stats.enabled = false;
            selected = null;
        }
        followDoid(selected);
    }
}
