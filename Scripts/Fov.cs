using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fov : MonoBehaviour
{
    public float fov = 90f;
    public Vector3 origin = Vector3.zero;
    public int numRays = 2;
    public float angle = 0f;
    public float viewDistance = 90f;

    public Vector3 angleToDir(float ang)
    {
        return new Vector3(Mathf.Sin(ang), Mathf.Cos(ang), 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        float angleIncrease = fov / numRays;
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;     

        Vector3[] vertices = new Vector3[numRays + 2];
        Vector2[] uv = new Vector2[numRays + 2];
        int[] triangles = new int[numRays*3];

        vertices[0] = origin;


        int vertexIndex = 0;
        int triangleIndex = 0;
        for(int i = 0; i < numRays; i++)
        {
            Vector3 vertex = origin + angleToDir(angle) * viewDistance;
            vertices[vertexIndex] = vertex;

            if(i > 0)
            {
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;
                triangleIndex += 3;
            }
            vertexIndex++;
            angle -= angleIncrease;
        }
        Debug.Log(vertices);


        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
