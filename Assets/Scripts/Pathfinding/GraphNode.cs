using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNode 
{
    private int x, y;

    private int gCost;
    private int hCost;
    private int fCost;

    public bool isObstacle;

    public GraphNode cameFromNode;

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

    public override string ToString()
    {
        return x + ", " + y;
    }
}
