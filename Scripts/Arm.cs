using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour
{

    void destruction()
    {
        Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        Invoke("destruction", 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
