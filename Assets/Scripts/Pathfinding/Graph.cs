using System;
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
    /// <param name="tilemap">The tilemap that is used to create the graph.</param>
    /// <param name="drawGraph">If true, draw graph.</param>
    public Graph(Tilemap tilemap, bool drawGraph)
    {
        this.tilemap = tilemap;

        // Get the bounds of the tilemap
        bounds = tilemap.cellBounds;
        
        // get width and height of the tilemap
        width = bounds.size.x;
        height = bounds.size.y;
        
        TileBase[] tiles = tilemap.GetTilesBlock(tilemap.cellBounds);

        GameObject textParent = new GameObject("TextParent");

        // create the 2D array for the GraphNodes with the width and height of the tilemap
        graphArray = new GraphNode[width, height];
        debugTextArray = new TextMesh[width, height];

        // loop through the tiles in the tilemap and create a GraphNode for each tile
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // get the tile at the current position
                TileBase tile = tiles[x + y * width];
                // get the neihbors of the current tile
                GraphNode[] neighbours = GetNeighbourList(new GraphNode(x, y));
                // create a new GraphNode with the current position and the neighbors 
                // and set the isObstacle node to true if the tile is not null
                GraphNode node = new GraphNode(x, y, (tile != null), neighbours);
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
    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) + bounds.min + tilemap.transform.position;
    }

    /// <summary>
    /// Gets the position in the graph of the given world position.
    /// </summary>
    /// <param name="worldPosition">The world position.</param>
    /// <returns>A Vector2Int.</returns>
    private Vector2Int GetXY(Vector3 worldPosition)
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
    /// Gets the neighbour list of a GraphNode
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>An array of GraphNodes.</returns>
    public GraphNode[] GetNeighbourList(GraphNode node)
    {
        // get the position of the given GraphNode
        Vector2Int pos = new Vector2Int(node.getX(), node.getY());
        GraphNode[] neighbours = new GraphNode[4];
        neighbours[0] = GetNode(pos.x, pos.y + 1);
        neighbours[1] = GetNode(pos.x + 1, pos.y);
        neighbours[2] = GetNode(pos.x, pos.y - 1);
        neighbours[3] = GetNode(pos.x - 1, pos.y);
        return neighbours;
    }
}
