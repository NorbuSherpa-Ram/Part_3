using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Ant
{
    private float AntTourLength = 0;
    private List<Connection> AntTravelledConnections = new List<Connection>();
    private GameObject StartNode;
    public Ant()
    {
    }
    public float GetAntTourLength()
    {
        return AntTourLength;
    }
    public void SetAntTourLength(float AntTourLength)
    {
        this.AntTourLength = AntTourLength;
    }
    public void AddAntTourLength(float AntTourLength)
    {
        this.AntTourLength += AntTourLength;
    }
    public void AddTravelledConnection(Connection aConnection)
    {
        AntTravelledConnections.Add(aConnection);
    }
    public List<Connection> GetConnections()
    {
        return AntTravelledConnections;
    }
    public GameObject GetStartNode()
    {
        return StartNode;
    }
    public void SetStartNode(GameObject StartNode)
    {
        this.StartNode = StartNode;
    }
}