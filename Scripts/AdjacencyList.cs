using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class AdjacencyList
{
    private Dictionary<(string, int), List<((string, int), float, int)>> _vertexDict = new Dictionary<(string, int), List<((string, int), float, int)>>();
    public int edgesCount;
    public int verticesCount;

    public List<((string, int), float, int)> AddVertex((string, int) key)
    {
        List<((string, int), float, int)> vertex = new List<((string, int), float, int)>();
        _vertexDict.Add(key, vertex);
        verticesCount++;

        return vertex;
    }

    public bool edgeExists((string, int) startKey, (string, int) endKey)
    {
        if (!_vertexDict.ContainsKey(endKey)) { return false; }
        List<((string, int), float, int)> vertex = ((List<((string, int), float, int)>)_vertexDict[endKey]);
        for(int i = 0; i < vertex.Count; i++)
        {
            if (vertex[i].Item1 == startKey)
            {
                return true;
            }
        }
        return false;
    }

    public void AddEdge((string, int) startKey, (string, int) endKey, float weight, int variable)
    {
        if(!edgeExists(startKey, endKey))
        {
            //if (!_vertexDict.ContainsKey(startKey))
            //            throw new ArgumentException("Cannot create edge from a non-existent start vertex.");

            List<((string, int), float, int)> endVertex = _vertexDict.ContainsKey(endKey) ? _vertexDict[endKey] : null;

            if (endVertex == null)
                endVertex = AddVertex(endKey);

            endVertex.Add((startKey, weight, variable));
            edgesCount++;
        }

    }

    public void RemoveVertex((string, int) key)
    {
        List<((string, int), float, int)> vertex = _vertexDict[key];

        //First remove the edges / adjacency entries
        int vertexNumAdjacent = vertex.Count;
        for (int i = 0; i < vertexNumAdjacent; i++)
        {
        (string, int) neighbourVertexKey = vertex[i].Item1;
            RemoveEdge(key, neighbourVertexKey);
        }

        //Lastly remove the vertex / adj. list
        _vertexDict.Remove(key);
        verticesCount--;
    }

    public void RemoveEdge((string, int) startKey, (string, int) endKey)
    {
        List<((string, int), float, int)> vertex = ((List<((string, int), float, int)>)_vertexDict[endKey]);
        for(int i = 0; i < vertex.Count; i++)
        {
            if (vertex[i].Item1 == startKey)
            {
                vertex.RemoveAt(i);
                edgesCount--;
            }
        }
    }

    public bool Contains((string, int) key)
    {
        return _vertexDict.ContainsKey(key);
    }

    public int VertexDegree((string, int) key)
    {
        return _vertexDict[key].Count;
    }

    public List<((string, int), float, int)> FindNeighbours((string, int) key)
    {
        return _vertexDict[key];
    }

    public List<(string, int)> vertices()
    {
        List<(string, int)> Vertices = new List<(string, int)>();
        foreach (var kvp in _vertexDict)
        {
            if(kvp.Value != null)
            {
                Vertices.Add(kvp.Key);
            }
        }
        return Vertices;
    }

    public List<((string, int), (string, int), float, int)> edges()
    {
        List<((string, int), (string, int), float, int)> Edges = new List<((string, int), (string, int), float, int)>();
        foreach(var kvp in _vertexDict)
        {
            (string, int) female = kvp.Key;
            foreach(((string, int) male, float weight, int variable) in kvp.Value)
            {
                Edges.Add((male, female, weight, variable));
            }
        }
        return Edges;
    }

    public void editEdge((string, int) male, (string, int) female, float weight, int variable)
    {
        RemoveEdge(male, female);
        AddEdge(male, female, weight, variable);
    }

    public (float, int) weightVariable((string, int) male, (string, int) female)
    {
        List<((string, int), float, int)> neighbors = FindNeighbours(female);
        foreach(((string, int) otherMale, float weight, int variable) in neighbors)
        {
            if(otherMale == male)
            {
                return (weight, variable);
            }
        }
        throw new ArgumentException("Edge doesn't exist.");
        return (0, 0);
    }

    public ((string, int), (string, int)) randomEdge()
    {
        
        List<((string, int), (string, int), float, int)> Edges = edges();

        GameObject worldd = GameObject.FindWithTag("World");
        random rand = worldd.GetComponent<random>();
        int index = rand.range(0, Edges.Count);

        ((string, int), (string, int), float, int) edge = Edges[index];

        return (edge.Item1, edge.Item2);
    }
}