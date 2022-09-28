using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour
{
    public int connectionMax = 30;
    public float weightMax = 15;
    public int connectionChance = 50;
    public int deletionChance = 20;
    public int variableChance = 10;
    public int inputSize;
    public int middleSize = 3;
    public int outputSize;
    public float mutationRate;
    public float weightMutation;
    public AdjacencyList adjConnections;
    public Dictionary<((string, int), (string, int)), (float, int)> weights;
    public Vector3 col;

    public int randomRange(int start, int end)
    {
        GameObject worldd = GameObject.FindWithTag("World");
        random rand = worldd.GetComponent<random>();
        return rand.range(start, end);
    }

    public float randomRange(float start, float end)
    {
        GameObject worldd = GameObject.FindWithTag("World");
        random rand = worldd.GetComponent<random>();
        return rand.range(start, end);
    }

    public void emptyBrain()
    {
        adjConnections = new AdjacencyList();
        for(int i = 1; i < inputSize; i++)
        {
            adjConnections.AddVertex(("input", i));
        }
        for (int i = 0; i < middleSize; i++)
        {
            adjConnections.AddVertex(("middle", i));
        }
        for (int i = 0; i < outputSize; i++)
        {
            adjConnections.AddVertex(("output", i));
        }
    }

    void Start()
    {
        col = new Vector3(randomRange(0, 1f), randomRange(0, 1f), randomRange(0, 1f));
    }

    public List<(string, int)> nodes()
    {
        return adjConnections.vertices();
    }

    public List<((string, int), (string, int), float, int)> connections()
    {
        return adjConnections.edges();
    }

    public (string, int) randomNode(string type)
    {
        int size = 0;
        switch (type)
        {
            case "input":
                size = inputSize;
                break;
            case "middle":
                size = middleSize;
                break;
            case "output":
                size = outputSize;
                break;
        }
        int index = randomRange(0, size);
        return (type, index);
    }

    public bool Dupe((string, int) male, (string, int) female)
    {
        return adjConnections.edgeExists(female, male) && male.Item1 == "middle" && female.Item1 == "middle";
    }

    public ((string, int), (string, int), float, int) randomAdjConnection()
    {
        (string, int) male = ("input", 0);
        (string, int) female = ("middle", 0);

        int connectType = randomRange(0, 4);

        if(connectType == 0)
        {
            male = randomNode("input");
            female = randomNode("middle");
        } else if(connectType == 1){
            male = randomNode("middle");
            female = randomNode("middle");
            //while(!circular(male, female, results, seen))
            //{
            //    results.Clear();
            //    seen.Clear();
            //   female = randomNode("middle");
            //}
        } else if (connectType == 2)
        {
            male = randomNode("middle");
            female = randomNode("output");
        } 

        float weight = randomRange(-weightMax, weightMax);
        int variable = -1;

        if (randomRange(0, 100) < variableChance)
        {
            variable = randomRange(0, inputSize);
        }

        if(adjConnections.edgesCount < connectionMax && !Dupe(male, female))
        {
            adjConnections.AddEdge(male, female, weight, variable);
        }
        return (male, female, weight, variable);
    }

    public ((string, int), (string, int), float, int) randomInpConnection((string, int) male)
    {  
        (string, int) female = randomNode("middle");
        
        float weight = randomRange(-weightMax, weightMax);
        int variable = -1;

        if (randomRange(0, 100) < variableChance)
        {
            variable = randomRange(0, inputSize);
        }

        if (adjConnections.edgesCount < connectionMax && !Dupe(male, female))
        {
            adjConnections.AddEdge(male, female, weight, variable);
        }
        return (male, female, weight, variable);
    }

    public void randomAdjBrain()
    {
        emptyBrain();
        for (int i = 0; i < inputSize; i++)
        {
            randomInpConnection(("input", i));
        }

        for (int i = 0; i < connectionMax / 3; i++)
        {
            if (randomRange(0f, 100f) < (float)connectionChance)
            {
                randomAdjConnection();
            }
        }
    }

    string nodeReadable((string, int) node)
    {
        string readable = "";
        if(node.Item1 == "input")
        {
            readable = readable + "I";
        } 
        else if (node.Item1 == "middle")
        {
            readable = readable + "M";
        }
        else if (node.Item1 == "output")
        {
            readable = readable + "O";
        }
        readable = readable + node.Item2.ToString();
        return readable;
    }

    public string toReadable()
    {
        string dna = "";
        List<((string, int), (string, int), float, int)> Connections = connections();
        foreach (((string, int), (string, int), float, int) connection in Connections)
        {
            dna = dna + nodeReadable(connection.Item1) + nodeReadable(connection.Item2) + connection.Item3.ToString();
        }
        return dna;
    }

    public void adjMutate(List<((string, int), (string, int), float, int)> parentConnections, float mutateRate)
    {
        emptyBrain();

        //Clone Parent Brain
        for (int i = 0; i < parentConnections.Count; i++)
        {
            (string, int) male = parentConnections[i].Item1;
            (string, int) female = parentConnections[i].Item2;
            float weight = parentConnections[i].Item3;
            int variable = parentConnections[i].Item4;

            adjConnections.AddEdge(male, female, weight, variable);
        }

        //Mutate Weights
        for (int i = 0; i < mutateRate * 100; i++)
        {
            if (randomRange(0f, 100f) < mutateRate * 100)
            {
                ((string, int) male, (string, int) female) = adjConnections.randomEdge();

                (float, int) wv = adjConnections.weightVariable(male, female);

                float weight = wv.Item1 + randomRange(-weightMutation, weightMutation);
                int variable = wv.Item2;

                if (weight != 0) { adjConnections.editEdge(male, female, weight, variable); }
            }
            if (randomRange(0f, 100f) < mutateRate * 50)
            {
                ((string, int) male, (string, int) female) = adjConnections.randomEdge();

                (float, int) wv = adjConnections.weightVariable(male, female);

                float weight = randomRange(-weightMax, weightMax);
                int variable = wv.Item2;

                if (weight != 0) { adjConnections.editEdge(male, female, weight, variable); }
            }
            if (randomRange(0f, 100f) < mutateRate * 50)
            {
                ((string, int) male, (string, int) female) = adjConnections.randomEdge();

                (float, int) wv = adjConnections.weightVariable(male, female);

                float weight = randomRange(-weightMax, weightMax);
                int variable = wv.Item2;

                if (weight == 0) { adjConnections.editEdge(male, female, weight, variable); }
            }
        }

        //Add and Delete Random Connections
        for (int i = 0; i < mutateRate * 100; i++)
        {
            if (randomRange(0f, 100f) < (float)connectionChance * mutateRate)
            {
                randomAdjConnection();
            }

            if (randomRange(0f, 100f) < (float)deletionChance * mutateRate)
            {
                ((string, int) male, (string, int) female) = adjConnections.randomEdge();
                (float, int) wv = adjConnections.weightVariable(male, female);
                int variable = wv.Item2;

                adjConnections.editEdge(male, female, 0, variable);
            }
        }
        
    }

        public void adjFuse(List<((string, int), (string, int), float, int)> maleConnections, List<((string, int), (string, int), float, int)> femaleConnections, float mutateRate)
        {
            int numConnections = Mathf.Min(femaleConnections.Count, maleConnections.Count);
            emptyBrain();

            for (int i = 0; i < numConnections; i++)
            {
                if (randomRange(0, 100) < 50)
                {
                    ((string, int), (string, int), float, int) newConnection = femaleConnections[i];
                    adjConnections.AddEdge(newConnection.Item1, newConnection.Item2, newConnection.Item3, newConnection.Item4);

                }
                else
                {
                    ((string, int), (string, int), float, int) newConnection = maleConnections[i];
                    adjConnections.AddEdge(newConnection.Item1, newConnection.Item2, newConnection.Item3, newConnection.Item4);
                }
            }

            for (int i = 0; i < mutateRate * 100; i++)
            {
                if (randomRange(0f, 100f) < mutateRate * 100)
                {
                    ((string, int) male, (string, int) female) = adjConnections.randomEdge();

                    (float, int) wv = adjConnections.weightVariable(male, female);

                    float weight = wv.Item1 + randomRange(-weightMutation, weightMutation);
                    int variable = wv.Item2;

                    if (weight != 0) { adjConnections.editEdge(male, female, weight, variable); }
                }
                if (randomRange(0f, 100f) < mutateRate * 50)
                {
                    ((string, int) male, (string, int) female) = adjConnections.randomEdge();

                    (float, int) wv = adjConnections.weightVariable(male, female);

                    float weight = randomRange(-weightMax, weightMax);
                    int variable = wv.Item2;

                    if (weight != 0) { adjConnections.editEdge(male, female, weight, variable); }
                }
                if (randomRange(0f, 100f) < mutateRate * 50)
                {
                    ((string, int) male, (string, int) female) = adjConnections.randomEdge();

                    (float, int) wv = adjConnections.weightVariable(male, female);

                    float weight = randomRange(-weightMax, weightMax);
                    int variable = wv.Item2;

                    if (weight == 0) { adjConnections.editEdge(male, female, weight, variable); }
                }
            }

            for (int i = 0; i < mutateRate * 100; i++)
            {
                if (randomRange(0f, 100f) < (float)connectionChance * mutateRate)
                {
                    randomAdjConnection();
                }

                if (randomRange(0f, 100f) < (float)deletionChance * mutateRate)
                {
                    ((string, int) male, (string, int) female) = adjConnections.randomEdge();
                    (float, int) wv = adjConnections.weightVariable(male, female);
                    int variable = wv.Item2;

                    adjConnections.editEdge(male, female, 0, variable);
                }
            }
        }
    

    float hashNum(float num)
    {
        var hash = Hash128.Compute(num);
        float prod = 1;
        foreach(char c in hash.ToString())
        {
            prod *= (float)c / 62;
        }
        return prod;
    }

    public Color color(int species, float age, float ageMax)
    {
        Vector4 newCol = ((new Vector4(hashNum(species) + col.x, hashNum(species + 1)  + col.y, hashNum(species + 2) + col.z, age  / ageMax ))).normalized * 100;
        return new Color(newCol.x, newCol.y, newCol.z, newCol.w);
    }
}
