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

    private bool isObstacle;

    // the GraphNode that came before this GraphNode in the path
    public GraphNode cameFromNode;
    
    public GraphNode(int x, int y, bool isObstacle)
    {
        this.x = x;
        this.y = y;
        this.isObstacle = isObstacle;
    }

    public GraphNode(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    
    public int GetX()
    {
        return x;
    }
    
    public int GetY()
    {
        return y;
    }

    public Vector2 GetPosition()
    {
        return new Vector2(x, y);
    }

    public int GetGCost()
    {
        return gCost;
    }
    
    public int GetHCost()
    {
        return hCost;
    }
    
    public int GetFCost()
    {
        return fCost;
    }
    
    public bool GetIsObstacle()
    {
        return isObstacle;
    }
    
    public void SetGCost(int gCost)
    {
        this.gCost = gCost;
    }
    
    public void SetHCost(int hCost)
    {
        this.hCost = hCost;
    }
    
    public void SetFCost(int fCost)
    {
        this.fCost = fCost;
    }
    
    public void SetIsObstacle(bool isObstacle)
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
