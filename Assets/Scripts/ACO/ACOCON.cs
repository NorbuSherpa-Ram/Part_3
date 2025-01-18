using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACOCON
{
    private float DefaultPheromone = 1.0f;

    private float Alpha = 1.0f;

    private float Beta = 0.0001f;

    // where 0 ≤ EvaporationFactor ≤ 1 is the evaporation factor of the pheromone.
    private float EvaporationFactor = 0.5f;

    // Q is a constant, it should be ≤ 1. 
    private float Q = 0.0006f;

    // Ants of agents moving through the graph. This class stores properties: Total distance and connections used.
    private List<Ant> Ants = new List<Ant>();

    // The generated route.
    private List<Connection> MyRoute = new List<Connection>();

    public ACOCON()
    {
    }

    public float GetDefaultPheromone()
    {
        return DefaultPheromone;
    }

    /*  IterationThreshold = Max number of iterations.
        TotalNumAnts = Total number of ants in the simulation.
        Connections = Connections between nodes.
        WaypointNodes = All the waypoint nodes in the waypoint graph used by the ACO algorithm.
     */
    private GameObject endNode;

    public List<Connection> ACO(int IterationThreshold, int TotalNumAnts, List<VisGraphWaypointManager> WaypointNodes, List<Connection> Connections, GameObject StartNode, GameObject endNode, int MaxPathLength)
    {
        this.endNode = endNode;

        if (StartNode == null)
        {
            Debug.Log("No Start node");
            return null;
        }

        // The node the ant is currently at.
        GameObject currentNode;

        // A list of all visited nodes.
        List<GameObject> VisitedNodes = new List<GameObject>();

        // Clear ants from previous runs.
        Ants.Clear();

        for (int i = 0; i < IterationThreshold; i++)
        {
            for (int i2 = 0; i2 < TotalNumAnts; i2++)
            {
                Ant aAnt = new Ant();

                // Randomly choose start node.
                currentNode = WaypointNodes[Random.Range(0, WaypointNodes.Count)].gameObject;
                aAnt.SetStartNode(currentNode);

                VisitedNodes.Clear();

                // Keep moving through the nodes until visited them all.
                // Keep looping until the number of nodes visited equals the number of nodes.
                while (VisitedNodes.Count < WaypointNodes.Count)
                {
                    // Get all connections from node.
                    List<Connection> ConnectionsFromNodeAndNotVisited = AllConnectionsFromNodeAndNotVisited(currentNode, Connections, VisitedNodes);

                    // Sum the product of the pheromone level and the visibility factor on all allowed paths.
                    float TotalPheromoneAndVisibility = CalculateTotalPheromoneAndVisibility(ConnectionsFromNodeAndNotVisited);

                    // Calculate the product of the pheromone level and the visibility factor of the proposed path.
                    // Loop through the paths and check if visited destination already.
                    foreach (Connection aConnection in ConnectionsFromNodeAndNotVisited)
                    {
                        // Not visited the path before.
                        float PathProbability = (Mathf.Pow(aConnection.GetPheromoneLevel(), Alpha) * Mathf.Pow((1 / aConnection.GetDistance()), Beta));
                        PathProbability = PathProbability / TotalPheromoneAndVisibility;

                        // Set path probability. Path probability is reset to zero at the end of each run.
                        aConnection.SetPathProbability(PathProbability);
                    }

                    // Travel down the path with the largest probability - or have a random choice if there are paths with equal probabilities.
                    // Loop through the paths and check if visited destination already.
                    Connection largestProbability = null;
                    if (ConnectionsFromNodeAndNotVisited.Count > 0)
                    {
                        largestProbability = ConnectionsFromNodeAndNotVisited[0];
                        for (int i3 = 1; i3 < ConnectionsFromNodeAndNotVisited.Count; i3++)
                        {
                            if (ConnectionsFromNodeAndNotVisited[i3].GetPathProbability() > largestProbability.GetPathProbability())
                            {
                                largestProbability = ConnectionsFromNodeAndNotVisited[i3];
                            }
                            else if (ConnectionsFromNodeAndNotVisited[i3].GetPathProbability() == largestProbability.GetPathProbability())
                            {
                                // Currently, 100% of the time chooses shortest connection if probabilities are the same.
                                if (ConnectionsFromNodeAndNotVisited[i3].GetDistance() < largestProbability.GetDistance())
                                {
                                    largestProbability = ConnectionsFromNodeAndNotVisited[i3];
                                }
                            }
                        }
                    }

                    // largestProbability contains the path to move down.
                    VisitedNodes.Add(currentNode);

                    if (largestProbability != null)
                    {
                        currentNode = largestProbability.GetToNode();

                        aAnt.AddTravelledConnection(largestProbability);
                        aAnt.AddAntTourLength(largestProbability.GetDistance());
                    }
                } //~END: While loop.

                Ants.Add(aAnt);
            }

            // Update pheromone by formula Δτij.
            // Loop through the paths and check if visited destination already.
            foreach (Connection aConnection in Connections)
            {
                float Sum = 0;
                foreach (Ant TmpAnt in Ants)
                {
                    List<Connection> TmpAntConnections = TmpAnt.GetConnections();
                    foreach (Connection tmpConnection in TmpAntConnections)
                    {
                        if (aConnection.Equals(tmpConnection))
                        {
                            Sum += Q / TmpAnt.GetAntTourLength();
                        }
                    }
                }

                float NewPheromoneLevel = (1 - EvaporationFactor) * aConnection.GetPheromoneLevel() + Sum;

                aConnection.SetPheromoneLevel(NewPheromoneLevel);

                // Reset path probability.
                aConnection.SetPathProbability(0);
            }
        }

        //TODO :: REMOVE THIS IF WANT TO SEE LOG OF WAY 
        //LogAnts();
        //LogRoute(StartNode, MaxPathLength, WaypointNodes, Connections);
        //LogConnections(Connections);

        MyRoute = GenerateRoute(StartNode, MaxPathLength, Connections);
        return MyRoute;
    }

    // Return all Connections from a node.
    private List<Connection> AllConnectionsFromNode(GameObject FromNode, List<Connection> Connections)
    {
        List<Connection> ConnectionsFromNode = new List<Connection>();

        foreach (Connection aConnection in Connections)
        {
            if (aConnection.GetFromNode() == FromNode)
            {
                ConnectionsFromNode.Add(aConnection);
            }
        }

        return ConnectionsFromNode;
    }

    // Return all Connections from a node that have not been visited.
    private List<Connection> AllConnectionsFromNodeAndNotVisited(GameObject FromNode, List<Connection> Connections, List<GameObject> VisitedList)
    {
        List<Connection> ConnectionsFromNode = new List<Connection>();

        foreach (Connection aConnection in Connections)
        {
            if (aConnection.GetFromNode() == FromNode)
            {
                if (!VisitedList.Contains(aConnection.GetToNode()))
                {
                    ConnectionsFromNode.Add(aConnection);
                }
            }
        }

        return ConnectionsFromNode;
    }

    // Sum the product of the pheromone level and the visibility factor on all allowed paths.
    private float CalculateTotalPheromoneAndVisibility(List<Connection> ConnectionsFromNodeAndNotVisited)
    {
        float TotalPheromoneAndVisibility = 0;

        // Loop through the paths and check if visited destination already.
        foreach (Connection aConnection in ConnectionsFromNodeAndNotVisited)
        {
            TotalPheromoneAndVisibility += (Mathf.Pow(aConnection.GetPheromoneLevel(), Alpha) * Mathf.Pow((1 / aConnection.GetDistance()), Beta));
        }

        return TotalPheromoneAndVisibility;
    }

    public List<Connection> GenerateRoute(GameObject StartNode, int MaxPath, List<Connection> Connections)
    {
        GameObject CurrentNode = StartNode;
        List<Connection> Route = new List<Connection>();

        Connection HighestPheromoneConnection = null;
        int PathCount = 1;

        while (CurrentNode != null)
        {
            List<Connection> AllFromConnections = AllConnectionsFromNode(CurrentNode, Connections);

            if (AllFromConnections.Count > 0)
            {
                HighestPheromoneConnection = AllFromConnections[0];

                foreach (Connection aConnection in AllFromConnections)
                {
                    if (aConnection.GetPheromoneLevel() > HighestPheromoneConnection.GetPheromoneLevel())
                    {
                        HighestPheromoneConnection = aConnection;
                    }
                }

                Route.Add(HighestPheromoneConnection);
                CurrentNode = HighestPheromoneConnection.GetToNode();
            }
            else
            {
                CurrentNode = null;
            }


            if (CurrentNode.Equals(endNode))
            {
                CurrentNode = null;
            }
            // If the current node is the start node at this point then we have looped through the path and should stop.
            // if (CurrentNode.Equals(StartNode))
            // {
            //     CurrentNode = null;
            // }

            // If the path count is greater than a max we should stop.
            if (PathCount > MaxPath)
            {
                CurrentNode = null;
            }

            PathCount++;
        }

        return Route;
    }

    // Log Connections.
    private void LogConnections(List<Connection> Connections)
    {
        foreach (Connection aConnection in Connections)
        {
            Debug.Log(">" + aConnection.GetFromNode().name + " | ---> " + aConnection.GetToNode().name + " = " + aConnection.GetPheromoneLevel());
        }
    }

    // Log Route
    private void LogRoute(GameObject StartNode, int MaxPath, GameObject[] WaypointNodes, List<Connection> Connections)
    {
        GameObject CurrentNode = null;
        foreach (GameObject GameObjectNode in WaypointNodes)
        {
            if (GameObjectNode.Equals(StartNode))
            {
                CurrentNode = GameObjectNode;
            }
        }

        Connection HighestPheromoneConnection = null;
        string Output = "Route (Q: " + Q + ", Alpha: " + Alpha + ", Beta: " + Beta + ", EvaporationFactor: " +
                        EvaporationFactor + ", DefaultPheromone: " + DefaultPheromone + "):\n";
        int PathCount = 1;
        while (CurrentNode != null)
        {
            List<Connection> AllFromConnections = AllConnectionsFromNode(CurrentNode, Connections);

            if (AllFromConnections.Count > 0)
            {
                HighestPheromoneConnection = AllFromConnections[0];

                foreach (Connection aConnection in AllFromConnections)
                {
                    if (aConnection.GetPheromoneLevel() > HighestPheromoneConnection.GetPheromoneLevel())
                    {
                        HighestPheromoneConnection = aConnection;
                    }
                }

                CurrentNode = HighestPheromoneConnection.GetToNode();

                Output += "| FROM: " + HighestPheromoneConnection.GetFromNode().name + ", TO: " + HighestPheromoneConnection.GetToNode().name +
                          " (Pheromone Level: " + HighestPheromoneConnection.GetPheromoneLevel() + ") | \n";
            }
            else
            {
                CurrentNode = null;
            }

            // If the current node is the start node at this point then we have looped through the path and should stop.
            if (CurrentNode.Equals(StartNode))
            {
                CurrentNode = null;

                Output += "HOME (Total Nodes:" + WaypointNodes.Length + ", Nodes in Route: " + PathCount + ").\n";
            }

            // If the path count is greater than a max we should stop.
            if (PathCount > MaxPath)
            {
                CurrentNode = null;

                Output += "MAX PATH (Total Nodes:" + WaypointNodes.Length + ", Nodes in Route: " + PathCount + ").\n";
            }

            PathCount++;
        }

        Debug.Log(Output);
    }

    // Log Route
    private void LogAnts()
    {
        string Output = "Ants (Q: " + Q + ", Alpha: " + Alpha + ", Beta: " + Beta + ", EvaporationFactor: " +
                        EvaporationFactor + ", DefaultPheromone: " + DefaultPheromone + "):\n";

        for (int i = 0; i < Ants.Count; i++)
        {
            Output += "Ant " + i + " - Start Node: " + Ants[i].GetStartNode().name + " | Tour Length: " + Ants[i].GetAntTourLength() + "\n";
        }

        Debug.Log(Output);
    }
}