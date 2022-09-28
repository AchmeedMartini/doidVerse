using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class updateText : MonoBehaviour
{
    public void textUpdate(float value)
    {
        this.GetComponent<TMPro.TextMeshProUGUI>().text = value.ToString();
    }
}
