using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class meshGen : MonoBehaviour
{
    public squareGrid grid;
    List<Vector3> vertices;
    List<int> triangles;

    public void GenerateMesh(int[,] map, float squareSize)
    {

        vertices = new List<Vector3>();
        triangles = new List<int>();
        grid = new squareGrid(map, squareSize);
        int width = grid.squares.GetLength(0);
        int height = grid.squares.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TriangulateSquare(grid.squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        MeshCollider meshc = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;

        meshc.sharedMesh = null;
        meshc.sharedMesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void TriangulateSquare(Square square)
    {
            switch (square.configuration)
            {
            case 0:
                break;
            
            case 1:
                MeshFromPoints(square.bottom , square.bottomLeft, square.left);
                break;

                case 2:
                MeshFromPoints(square.right , square.bottomRight, square.bottom);
                    break;

                case 4:
                MeshFromPoints(square.top , square.topRight, square.right);
                    break;
                case 8:
                MeshFromPoints(square.topLeft , square.top, square.left);
                    break;

                case 3:
                MeshFromPoints(square.right , square.bottomRight, square.bottomLeft, square.left);
                    break;

                case 6:
                MeshFromPoints(square.top , square.topRight, square.bottomRight, square.bottom);
                    break;

                case 9:
                MeshFromPoints(square.topLeft , square.top, square.bottom, square.bottomLeft);
                    break;

                case 12:
                MeshFromPoints(square.topLeft , square.topRight, square.right, square.left);
                    break;

                case 5:
                MeshFromPoints(square.top , square.topRight, square.right, square.bottom, square.bottomLeft, square.left);
                    break;

                case 10:
                MeshFromPoints(square.topLeft , square.top, square.right, square.bottomRight, square.bottom, square.left);
                    break;

                case 7:
                MeshFromPoints(square.top , square.topRight, square.bottomRight, square.bottomLeft, square.left, square.left);
                    break;

                case 11:
                MeshFromPoints(square.topLeft , square.top, square.right, square.bottomRight, square.bottomLeft);
                    break;

                case 13:
                MeshFromPoints(square.topLeft , square.topRight, square.right, square.bottom, square.bottomLeft);
                    break;

                case 14:
                MeshFromPoints(square.topLeft , square.topRight, square.bottomRight, square.bottom, square.left);
                    break;

                case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                    break;

                }
    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
        {
            createTriangles(points[0], points[1], points[2]);
        }
        if (points.Length >= 4)
        {
            createTriangles(points[0], points[2], points[3]);
        }
        if (points.Length >= 5)
        {
            createTriangles(points[0], points[3], points[4]);
        }
        if (points.Length >= 6)
        {
            createTriangles(points[0], points[4], points[5]);
        }
    }

    void AssignVertices(Node[] points)
    {
        for(int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                Vector3 Pos = new Vector3(points[i].pos.x, points[i].pos.y, Random.Range(0, 1));
                points[i].vertexIndex = vertices.Count;
                vertices.Add(Pos);
            }
        }
    }

    void createTriangles(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);
    }
    /*
    void OnDrawGizmos()
    {

        if(grid != null)
        {
            int width = grid.squares.GetLength(0);
            int height = grid.squares.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (grid.squares[x, y].topLeft.active ? Color.black : Color.white);
                    Gizmos.DrawCube(grid.squares[x, y].topLeft.pos, Vector2.one * .4f);

                    Gizmos.color = (grid.squares[x, y].topRight.active ? Color.black : Color.white);
                    Gizmos.DrawCube(grid.squares[x, y].topRight.pos, Vector2.one * .4f);

                    Gizmos.color = (grid.squares[x, y].bottomRight.active ? Color.black : Color.white);
                    Gizmos.DrawCube(grid.squares[x, y].bottomRight.pos, Vector2.one * .4f);

                    Gizmos.color = (grid.squares[x, y].bottomLeft.active ? Color.black : Color.white);
                    Gizmos.DrawCube(grid.squares[x, y].bottomLeft.pos, Vector2.one * .4f);

                    Gizmos.color = Color.grey;
                    Gizmos.DrawCube(grid.squares[x, y].top.pos, Vector2.one * .15f);
                    Gizmos.DrawCube(grid.squares[x, y].left.pos, Vector2.one * .15f);
                    Gizmos.DrawCube(grid.squares[x, y].bottom.pos, Vector2.one * .15f);
                    Gizmos.DrawCube(grid.squares[x, y].right.pos, Vector2.one * .15f);
                }
            }
        }
    }*/

    public class squareGrid
    {
        public Square[,] squares;

        public squareGrid(int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for(int x = 0; x < nodeCountX; x++)
            {
                for(int y = 0; y < nodeCountY; y++)
                {
                    Vector2 Pos = new Vector2(-mapWidth / 2 + x * squareSize + squareSize / 2, -mapHeight / 2 + y * squareSize + squareSize / 2);
                    controlNodes[x, y] = new ControlNode(Pos, map[x, y] == 1, squareSize);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }

    public class Square {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node top, left, bottom, right;
        public int configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            top = topLeft.right;
            left = bottomLeft.above;
            bottom = bottomLeft.right;
            right = bottomRight.above;

            if (topLeft.active)
            {
                configuration += 8;
            }
            if (topRight.active)
            {
                configuration += 4;
            }
            if (bottomRight.active)
            {
                configuration += 2;
            }
            if (bottomLeft.active)
            {
                configuration += 1;
            }
        }
    }

    public class Node
    {
        public Vector2 pos;
        public int vertexIndex = -1;
        public Node(Vector3 _pos)
        {
            pos = _pos;
        }
    }
    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;
        public ControlNode(Vector2 _pos, bool _active, float squareSize) : base(_pos)
        {
            active = _active;
            above = new Node(_pos + Vector2.up * squareSize / 2f);
            right = new Node(_pos + Vector2.right * squareSize / 2f);
        }
    }

}
