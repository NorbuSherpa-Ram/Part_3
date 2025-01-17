using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Connection
{
    private float Cost = 0;
    public GameObject FromNode;
    public GameObject ToNode;
    private float distance;
    private float PheromoneLevel;
    private float PathProbability;


    public void SetConnection(GameObject FromNode, GameObject ToNode, float DefaultPheromoneLevel)
    {
        this.FromNode = FromNode;
        this.ToNode = ToNode;
        distance = Vector3.Distance(FromNode.transform.position, ToNode.transform.position);
        PheromoneLevel = DefaultPheromoneLevel;
        PathProbability = 0;
    }


    public float GetCost()
    {
        if (Cost == 0)
        {
            Cost = Vector3.Distance(FromNode.transform.position, ToNode.transform.position);
        }

        return Cost;
    }

    public GameObject GetFromNode()
    {
        return FromNode;
    }

    public void SetFromNode(GameObject FromNode)
    {
        this.FromNode = FromNode;
        Cost = 0;
    }

    public GameObject GetToNode()
    {
        return ToNode;
    }

    public void SetToNode(GameObject ToNode)
    {
        this.ToNode = ToNode;
        Cost = 0;
    }

    public float GetDistance()
    {
        return distance;
    }

    public float GetPheromoneLevel()
    {
        return PheromoneLevel;
    }

    public void SetPheromoneLevel(float PheromoneLevel)
    {
        this.PheromoneLevel = PheromoneLevel;
    }

    public float GetPathProbability()
    {
        return PathProbability;
    }

    public void SetPathProbability(float PathProbability)
    {
        this.PathProbability = PathProbability;
    }
}