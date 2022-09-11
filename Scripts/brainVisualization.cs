using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


public class brainVisualization : MonoBehaviour
{
    public Sprite NodeSprite;
    public List<(string, int)> nodes;
    public Brain brain;
    public Doid doid;
    public RectTransform graphContainer;
    public Image image;
    public GameObject connection;
    public bool show = false;

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

    Vector2 nodePos((string, int) node)
    {
        float xPos = 0;
        float yPos = node.Item2 * 10;
        if (node.Item1 == "input")
        {
            Vector2 posNormal = (new Vector2(hashNum(node.Item2 + doid.species.GetHashCode() / 10000), hashNum(node.Item2 - doid.species.GetHashCode() / 10000))).normalized;
            float mag = 150;
            float dir = posNormal.y * 360;

            Vector2 offset = mag * (new Vector2(Mathf.Sin(dir), Mathf.Cos(dir)));

            xPos = (image.GetComponent<RectTransform>().rect.width / 2) + offset.x;
            yPos = (image.GetComponent<RectTransform>().rect.height / 2) + offset.y;
        }
        else if (node.Item1 == "middle")
        {
            Vector2 posNormal = (new Vector2(hashNum(node.Item2 + doid.species.GetHashCode() / 10000), hashNum(node.Item2 - doid.species.GetHashCode() / 10000))).normalized;
            float mag = 100 + (posNormal.x * 30) - 10;
            float dir = posNormal.y * 360;

            Vector2 offset = mag * (new Vector2(Mathf.Sin(dir), Mathf.Cos(dir)));

            xPos = (image.GetComponent < RectTransform > ().rect.width / 2) + offset.x;
            yPos = (image.GetComponent<RectTransform>().rect.height / 2) + offset.y;
        }
        else if (node.Item1 == "output")
        {
            Vector2 posNormal = (new Vector2(hashNum(node.Item2 + doid.species.GetHashCode() / 10000), hashNum(node.Item2 - doid.species.GetHashCode() / 10000))).normalized;
            float mag = 50;
            float dir = posNormal.y * 360;

            Vector2 offset = mag * (new Vector2(Mathf.Sin(dir), Mathf.Cos(dir)));

            xPos = (image.GetComponent<RectTransform>().rect.width / 2) + offset.x;
            yPos = (image.GetComponent<RectTransform>().rect.height / 2) + offset.y;
        }
        return new Vector2(xPos, yPos);
    }

    void drawNode((string, int) node)
    {
        Color col = new Color(0, 0, 0, 0);
        float xPos = 0;
        if(node.Item1 == "input")
        {
            col = new Color(0, 0, 1, doid.inputs[node.Item2] + .05f);
        }
        else if (node.Item1 == "middle")
        {
            col = new Color(0, 1, 0, doid.middle[node.Item2] + .05f);
        }
        else if (node.Item1 == "output")
        {
            col = new Color(1, 0, 0, doid.outputs[node.Item2] + .05f);
        }
        GameObject circle = new GameObject("circle", typeof(Image));
        circle.transform.SetParent(graphContainer, false);
        circle.GetComponent<Image>().sprite = NodeSprite;
        circle.GetComponent<Image>().color = col;
        RectTransform rect = circle.GetComponent<RectTransform>();
        rect.anchoredPosition = nodePos(node);
        rect.sizeDelta = new Vector2(11, 11);
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);


    }

    public static double Exp(float value)
    {
        double val = (double)value;
        long tmp = (long)(1512775 * val + 1072632447);
        return BitConverter.Int64BitsToDouble(tmp << 32);
    }

    static float Sigmoid(float value)
    {
        float k = (float)Exp(value);
        if (float.IsNaN(k)) { return 0; }
        return k / (1.0f + k);
    }

    Vector2 nodeImgPos((string, int) node)
    {
        Vector2 nodepos = nodePos(node);
        foreach(Transform child in transform)
        {
            if (child.gameObject.GetComponent<RectTransform>().anchoredPosition == nodepos)
            {
                return child.position;
            }
        }
        return new Vector2(0, 0);
    }

    public Color connectColor((string, int) node, float weight)
    {
        Color col = Color.white;
        if (node.Item1 == "input")
        {
            if(weight < 0) {col = new Color((Mathf.Abs(weight) / doid.GetComponent<Brain>().weightMax),0, 0, .25f + .75f*doid.inputs[node.Item2]); }
            if (weight == 0) { col = new Color(0, 0, 0, 1); }
            if (weight > 0) { col = new Color(0, 0, (Mathf.Abs(weight) / doid.GetComponent<Brain>().weightMax), .25f + .75f * doid.inputs[node.Item2]); }

        }
        else if (node.Item1 == "middle")
        {
            if (weight < 0) { col = new Color((Mathf.Abs(weight) / doid.GetComponent<Brain>().weightMax), 0, 0, .25f + .75f * doid.inputs[node.Item2]); }
            if (weight == 0) { col = new Color(0, 0, 0, 1); }
            if (weight > 0) { col = new Color(0, 0,(Mathf.Abs(weight) / doid.GetComponent<Brain>().weightMax), .25f + .75f * doid.inputs[node.Item2]); }
            //col = new Color(0, 1, 0, doid.middle[node.Item2]);
        }
        else if (node.Item1 == "output")
        {
            if (weight < 0) { col = new Color((Mathf.Abs(weight) / doid.GetComponent<Brain>().weightMax), 0, 0, .25f + .75f * doid.inputs[node.Item2]); }
            if (weight == 0) { col = new Color(0, 0, 0, 1); }
            if (weight > 0) { col = new Color(0, 0, (Mathf.Abs(weight) / doid.GetComponent<Brain>().weightMax), .25f + .75f * doid.inputs[node.Item2]); }
            //col = new Color(1, 0, 0, doid.outputs[node.Item2]);
        }
        return col;
    }

    void drawConnection((string, int) node1, (string, int) node2, float weight) 
    {
        Color col1 = connectColor(node1, weight);
        Color col2 = connectColor(node2, weight);
        GameObject con = Instantiate(connection, nodePos(node1), Quaternion.identity) as GameObject;

        Vector2 node1ImgPos = nodeImgPos(node1);
        Vector2 node2ImgPos = nodeImgPos(node2);
        if(node2ImgPos == Vector2.zero)
        {
            node2ImgPos = node1ImgPos;
        } 
        if(node1ImgPos == Vector2.zero)
        {
            node1ImgPos = node2ImgPos;
        }
        con.GetComponent<LineRenderer>().SetWidth((Mathf.Abs(weight) - Camera.main.orthographicSize) / 200, (Mathf.Abs(weight) - Camera.main.orthographicSize) / 200);
        con.GetComponent<LineRenderer>().SetPosition(0, node1ImgPos);
        con.GetComponent<LineRenderer>().SetPosition(1, node2ImgPos);
        con.GetComponent<LineRenderer>().SetColors(col1, col2);
        //yield return new WaitForSeconds(1f);
        //Destroy(arm);
       
    }

    void place()
    {
        foreach((string,int) node in nodes)
        {
            drawNode(node);
        }
        List<((string, int), (string, int), float, int)> connections = brain.connections();
        foreach (((string, int), (string, int), float, int) connection in connections )
        {
            drawConnection(connection.Item1, connection.Item2, connection.Item3);
        }
    }

    void delete()
    {
        foreach(Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void visualize()
    {
        image.enabled = true;
        nodes = brain.nodes();
        show = true;
    }

    public void unvisualize()
    {
        show = false;
    }

    void Update()
    {
        delete();
        if (show)
        {
            
            place();
            
        }
        else
        {
            image.enabled = false;
        }
    }

}
