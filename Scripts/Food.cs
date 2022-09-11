using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    void Update()
    {
        Boundary bounds = FindObjectOfType<Boundary>();
        if (bounds.outOfBounds(gameObject.transform.position))
        {
            gameObject.transform.position = bounds.wrappedPosition(gameObject.transform.position);
        }
    }
}
