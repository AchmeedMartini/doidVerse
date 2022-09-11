using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class start : MonoBehaviour
{
    public GameObject world;
    public Image menu;
    public TMPro.TextMeshProUGUI startButton;
    public GameObject population;
    public GameObject foodAmount;
    public GameObject mapSize;
    public GameObject caveSize;
    public GameObject mapSeed;
    public GameObject simSeed;


    void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(TaskOnClick);
    }

    void inputParams()
    {
        world.GetComponent<World>().doidMin = (int)population.transform.GetChild(0).GetComponent<Slider>().value;
        world.GetComponent<World>().doidMax = (int)population.transform.GetChild(1).GetComponent<Slider>().value;
        world.GetComponent<World>().foodMin = (int)foodAmount.transform.GetChild(0).GetComponent<Slider>().value;
        world.GetComponent<World>().foodMax = (int)foodAmount.transform.GetChild(1).GetComponent<Slider>().value;
        world.GetComponent<mapGen>().width = (int)mapSize.transform.GetChild(0).GetComponent<Slider>().value;
        world.GetComponent<mapGen>().height = (int)mapSize.transform.GetChild(1).GetComponent<Slider>().value;
        world.GetComponent<mapGen>().randomFillPercent = (int)(100 - caveSize.transform.GetChild(0).GetComponent<Slider>().value);
    }

    void clearMenu()
    {
        population.SetActive(false);
        foodAmount.SetActive(false);
        mapSize.SetActive(false);
        caveSize.SetActive(false);
        mapSeed.SetActive(false);
        simSeed.SetActive(false);
        menu.enabled = false;
        startButton.enabled = false;
        this.enabled = false;
    }

    void TaskOnClick()
    {
        Debug.Log(world.GetComponent<World>().randomValue());
        inputParams();
        clearMenu();
        world.GetComponent<mapGen>().genMap();
        world.GetComponent<World>().worldStats.enabled = true;
        world.GetComponent<World>().year = 0;
    }
}
