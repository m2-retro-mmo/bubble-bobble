using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// This class handles the Pathfinding.
/// </summary>
public class Pathfinding
{
    // the graph that is used for pathfinding
    private Graph graph;

    /// <summary>
    /// Initializes a new instance of the <see cref="Pathfinding"/> class.
    /// </summary>
    /// <param name="tilemap">The tilemap that is used to create the graph structure</param>
    public Pathfinding(Graph graph)
    {
        this.graph = graph;
    }

    /// <summary>
    /// Finds the path from the start to the end position using the A* algorithm.
    /// </summary>
    /// <param name="startPos">The start pos.</param>
    /// <param name="endPos">The end pos.</param>
    /// <returns>A list of GraphNodes.</returns>
    public List<GraphNode> FindPath(Vector3 startPos, Vector3 endPos)
    {
        // get the start and end node from the graph
        GraphNode startNode = graph.GetNode(startPos);
        GraphNode endNode = graph.GetNode(endPos);
        startNode.cameFromNode = null;

        // create a list for the open and closed nodes
        List<GraphNode> openList = new List<GraphNode>();
        HashSet<GraphNode> closedList = new HashSet<GraphNode>();

        openList.Add(startNode);

        // continue until the open list is empty
        while (openList.Count > 0)
        {
            GraphNode currentNode = openList[0];
            // loop through the open list and get the node with the lowest fCost
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].GetFCost() < currentNode.GetFCost() || openList[i].GetFCost() == currentNode.GetFCost() && openList[i].GetHCost() < currentNode.GetHCost())
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // if the current node is the end node, return the path
            if (currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            // loop through the neighbors of the current node
            foreach (GraphNode neighbourNode in graph.GetNeighboursOfNode(currentNode))
            {
                // if the neighbor is not walkable or is in the closed list, ignore it
                if (neighbourNode.GetIsObstacle() || closedList.Contains(neighbourNode)) // TODO bounds of tile map
                {
                    continue;
                }

                // calculate the new movement cost to the neighbor
                int newMovementCostToNeighbour = currentNode.GetGCost() + GetDistance(currentNode, neighbourNode);
                // if the new movement cost to the neighbor is lower than the old one or the neighbor is not in the open list
                if (newMovementCostToNeighbour < neighbourNode.GetGCost() || !openList.Contains(neighbourNode))
                {
                    // set the cost values for the neighbor
                    neighbourNode.SetGCost(newMovementCostToNeighbour);
                    neighbourNode.SetHCost(GetDistance(neighbourNode, endNode));
                    // set the current node as the neighbor's previous node for reconstructing the path
                    neighbourNode.cameFromNode = currentNode;

                    // if the neighbor is not in the open list, add it
                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        // if the path could not be found, return null
        return null;
    }

    /// <summary>
    /// Calculates the path beginning at the end node.
    /// </summary>
    /// <param name="endNode">The end node.</param>
    /// <returns>A list of GraphNodes.</returns>
    private List<GraphNode> CalculatePath(GraphNode endNode)
    {
        // create a list for the path
        List<GraphNode> path = new List<GraphNode>();
        
        path.Add(endNode);
        GraphNode currentNode = endNode;

        // loop through the nodes and add them to the path
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            // get the previous node of the current node
            currentNode = currentNode.cameFromNode;
        }
        // reverse the path so that it starts at the start node
        path.Reverse();
        
        return path;
    }

    /// <summary>
    /// Gets the distance between two nodes.
    /// </summary>
    /// <param name="nodeA">The node a.</param>
    /// <param name="nodeB">The node b.</param>
    /// <returns>An int.</returns>
    private int GetDistance(GraphNode nodeA, GraphNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.GetX() - nodeB.GetX());
        int dstY = Mathf.Abs(nodeA.GetY() - nodeB.GetY());
        
        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public Graph GetGraph()
    {
        return graph;
    }
}
