using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// The graph class that holds the GraphNodes.
/// </summary>
public class Graph
{
    // the size of the graph consisting of the number of nodes in the x and y direction
    private int width, height;

    // the tilemap that is used to create the graph
    private Tilemap tilemap;

    // the bounds of the tilemap
    private BoundsInt bounds;

    // the 2D array of GraphNodes
    private GraphNode[,] graphArray;

    private TextMesh[,] debugTextArray;

    /// <summary>
    /// Initializes a new instance of the <see cref="Graph"/> class.
    /// </summary>
    /// <param name="obstacle_tilemap">The tilemap that is used to create the graph.</param>
    /// <param name="drawGraph">If true, draw graph.</param>
    public Graph(Map map, bool drawGraph)
    {
        this.tilemap = map.obstacleTilemap;

        // Get the bounds of the tilemap
        bounds = map.obstacleTilemap.cellBounds;
        
        // get width and height of the tilemap
        width = map.GetWidth();
        height = map.GetHeight();

        Boolean[,] isObstacle = GetNonWalkableFields(map.floorEnvironment);

        GameObject textParent = new GameObject("TextParent");

        // create the 2D array for the GraphNodes with the width and height of the tilemap
        graphArray = new GraphNode[width, height];
        debugTextArray = new TextMesh[width, height];

        // loop through the tiles in the tilemap and create a GraphNode for each tile
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // create a new GraphNode with the current position and the neighbours 
                // and set the isObstacle node to true if the tile is not null
                GraphNode node = new GraphNode(x, y, isObstacle[x,y]);
                
                // add the GraphNode to the 2D array
                graphArray[x, y] = node;
                
                if (drawGraph)
                {
                    // create a new TextMesh object with the current position
                    debugTextArray[x, y] = UtilsClass.CreateWorldText(graphArray[x, y].ToString(), textParent.transform, GetWorldPosition(x, y) + new Vector3(0.5f, 0.5f), 50, Color.white, TextAnchor.MiddleCenter);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
            }
        }

        if (drawGraph)
        {
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
        }
    }

    /// <summary>
    /// Gets the world position of the GraphNode at the given position.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>A Vector3.</returns>
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) + bounds.min + tilemap.transform.position;
    }

    public Vector3 GetWorldPosition(GraphNode node)
    {
        return GetWorldPosition(node.GetX(), node.GetY());
    }

    /// <summary>
    /// Gets the position in the graph of the given world position.
    /// </summary>
    /// <param name="worldPosition">The world position.</param>
    /// <returns>A Vector2Int.</returns>
    public Vector2Int GetXY(Vector3 worldPosition)
    {
        // calculates the rounded position of the given world position
        Vector2Int position = new Vector2Int(
            Mathf.FloorToInt(worldPosition.x - tilemap.transform.position.x - bounds.min.x), 
            Mathf.FloorToInt(worldPosition.y - tilemap.transform.position.y - bounds.min.y));
        return position;
    }

    /// <summary>
    /// Gets the GraphNode at the given position in the graph.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>A GraphNode.</returns>
    public GraphNode GetNode(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return graphArray[x, y];
        }
        else
        {
            return default(GraphNode);
        }
    }
    
    /// <summary>
    /// Gets the GraphNode at the given world position.
    /// </summary>
    /// <param name="worldPosition">The world position.</param>
    /// <returns>A GraphNode.</returns>
    public GraphNode GetNode(Vector3 worldPosition)
    {
        // get the position in the graph of the given world position
        Vector2Int pos = GetXY(worldPosition);
        return GetNode(pos.x, pos.y);
    }

    public Boolean[,] GetNonWalkableFields(EnvironmentType[,] grid)
    {
        Boolean[,] nonWalkables = new Boolean[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++) {
                if (grid[x, y] == EnvironmentType.Water) nonWalkables[x,y] = true;
                else nonWalkables[x, y] = false;
            }
        }
        return nonWalkables;
    }

    /// <summary>
    /// Gets the text  of the <param name="debugArray</param> at the given position.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>A string.</returns>
    public string GetText(int x, int y)
    {
        return debugTextArray[x, y].text;
    }

    /// <summary>
    /// Sets the GraphNode at the given position in the graph.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="node">The GraphNode.</param>
    public void SetNode(int x, int y, GraphNode node)
    {
        if (0 <= x && x < width && 
            0 <= y && y < height)
        {
            graphArray[x, y] = node;
        }
    }

    /// <summary>
    /// Sets the GraphNode at the given world position. 
    /// </summary>
    /// <param name="worldPosition">The world position.</param>
    /// <param name="node">The node.</param>
    public void SetNode(Vector3 worldPosition, GraphNode node)
    {
        // get the position in the graph of the given world position
        Vector2Int pos = GetXY(worldPosition);
        SetNode(pos.x, pos.y, node);
    }

    /// <summary>
    /// Sets the text at the given position.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="text">The text.</param>
    public void SetText(int x, int y, string text)
    {
        debugTextArray[x, y].text = text;
    }

    /// <summary>
    /// Resets the graph by setting all the GraphNodes to not visited 
    /// </summary>
    public void ResetGraph()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                graphArray[x, y].cameFromNode = null;
            }
        }
    }

    /// <summary>
    /// Gets the neighbour list of a GraphNode
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>An array of GraphNodes.</returns>
    public List<GraphNode> GetNeighboursOfNode(GraphNode currentNode)
    {
        List<GraphNode> neighbours = new List<GraphNode>();

        for (int x = currentNode.GetX() - 1; x <= currentNode.GetX() + 1; x++)
        {
            for (int y = currentNode.GetY() - 1; y <= currentNode.GetY() + 1; y++)
            {
                if (x == currentNode.GetX() && y == currentNode.GetY())
                {
                    continue;
                }
                if (x < 0 || x >= graphArray.GetLength(0) || y < 0 || y >= graphArray.GetLength(1)) // edge cases, node is outside of bounds
                {
                    continue;
                }
                neighbours.Add(graphArray[x, y]);
            }
        }
        return neighbours;
    }
}
