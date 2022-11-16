using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles the GraphNode object. 
/// </summary>
public class GraphNode 
{
    // the position of the node in the graph
    private int x, y;

    // the cost of moving from the starting point to this GraphNode, following the path generated to get there
    private int gCost;
    // the heuristic value that estimates the cost of the cheapest path from this GraphNode to the goal
    private int hCost;
    // the sum of gCost and hCost
    private int fCost;

    public bool isObstacle;

    // the GraphNode that came before this GraphNode in the path
    public GraphNode cameFromNode;

    // a list of all the GraphNodes that are adjacent to this GraphNode
    private GraphNode[] neighbours;
    
    public GraphNode(int x, int y, bool isObstacle, GraphNode[] neighbors)
    {
        this.x = x;
        this.y = y;
        this.isObstacle = isObstacle;
        this.neighbours = neighbors;
    }
    
    public GraphNode(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    
    public int getX()
    {
        return x;
    }
    
    public int getY()
    {
        return y;
    }
    
    public int getGCost()
    {
        return gCost;
    }
    
    public int getHCost()
    {
        return hCost;
    }
    
    public int getFCost()
    {
        return fCost;
    }
    
    public GraphNode[] getNeighbours()
    {
        return neighbours;
    }
    
    public bool GetIsObstacle()
    {
        return isObstacle;
    }
    
    public void setGCost(int gCost)
    {
        this.gCost = gCost;
    }
    
    public void setHCost(int hCost)
    {
        this.hCost = hCost;
    }
    
    public void setFCost(int fCost)
    {
        this.fCost = fCost;
    }
    
    public void setNeighbours(GraphNode[] neighbours)
    {
        this.neighbours = neighbours;
    }
    
    public void setIsObstacle(bool isObstacle)
    {
        this.isObstacle = isObstacle;
    }

    /// <summary>
    /// generates a string representation of the GraphNode consisting of its x and y values
    /// </summary>
    /// <returns>A string.</returns>
    public override string ToString()
    {
        return x + ", " + y;
    }
}
