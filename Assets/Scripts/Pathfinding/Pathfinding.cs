using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinding
{
    private Graph graph;

    public Pathfinding(Tilemap tilemap)
    {
        graph = new Graph(tilemap, true);
    }

    public List<GraphNode> FindPath(Vector3 startPos, Vector3 endPos)
    {
        GraphNode startNode = graph.GetNode(startPos);
        GraphNode endNode = graph.GetNode(endPos);

        List<GraphNode> openList = new List<GraphNode>();
        HashSet<GraphNode> closedList = new HashSet<GraphNode>();

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            GraphNode currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].getFCost() < currentNode.getFCost() || openList[i].getFCost() == currentNode.getFCost() && openList[i].getHCost() < currentNode.getHCost())
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            foreach (GraphNode neighbourNode in graph.GetNeighbourList(currentNode))
            {
                if (neighbourNode.isObstacle || closedList.Contains(neighbourNode))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.getGCost() + GetDistance(currentNode, neighbourNode);
                if (newMovementCostToNeighbour < neighbourNode.getGCost() || !openList.Contains(neighbourNode))
                {
                    neighbourNode.setGCost(newMovementCostToNeighbour);
                    neighbourNode.setHCost(GetDistance(neighbourNode, endNode));
                    neighbourNode.cameFromNode = currentNode;

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        return null;
    }

    private List<GraphNode> CalculatePath(GraphNode endNode)
    {
        List<GraphNode> path = new List<GraphNode>();
        path.Add(endNode);
        GraphNode currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private int GetDistance(GraphNode nodeA, GraphNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.getX() - nodeB.getX());
        int dstY = Mathf.Abs(nodeA.getY() - nodeB.getY());

        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
