using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Graph
{
    private int width;
    private int height;
    private Tilemap tilemap;
    private BoundsInt bounds;
    private GraphNode[,] graphArray;

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

        graphArray = new GraphNode[width, height];
        TextMesh[,] debugTextArray = new TextMesh[width, height];

        // loop through the tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileBase tile = tiles[x + y * width];
                GraphNode[] neighbours = new GraphNode[4];
                neighbours = GetNeighbourList(new GraphNode(x, y));
                GraphNode node = new GraphNode(x, y, (tile != null), neighbours);
                graphArray[x, y] = node;
                
                if (drawGraph)
                {
                    UtilsClass.CreateWorldText(graphArray[x, y].ToString(), textParent.transform, GetWorldPosition(x, y) + new Vector3(0.5f, 0.5f), 50, Color.white, TextAnchor.MiddleCenter);
                    //debugTextArray[x, y] = "1";
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

    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) + bounds.min + tilemap.transform.position;
    }

    private Vector2Int GetXY(Vector3 worldPosition)
    {
        Vector2Int position = new Vector2Int(
            Mathf.FloorToInt(worldPosition.x - tilemap.transform.position.x - bounds.min.x), 
            Mathf.FloorToInt(worldPosition.y - tilemap.transform.position.y - bounds.min.y));
        return position;
    }

    public GraphNode GetValue(int x, int y)
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

    public GraphNode GetValue(Vector3 worldPosition)
    {
        Vector2Int pos = GetXY(worldPosition);
        return GetValue(pos.x, pos.y);
    }

    public void SetValue(int x, int y, GraphNode value)
    {
        if (0 <= x && x < width && 
            0 <= y && y < height)
        {
            graphArray[x, y] = value;
        }
    }

    public void SetValue(Vector3 worldPosition, GraphNode value)
    {
        Vector2Int pos = GetXY(worldPosition);
        SetValue(pos.x, pos.y, value);
    }

    public GraphNode GetNode(Vector3 worldPosition)
    {
        Vector2Int pos = GetXY(worldPosition);
        return GetValue(pos.x, pos.y);
    }

    public GraphNode[] GetNeighbourList(GraphNode node)
    {
        Vector2Int pos = new Vector2Int(node.getX(), node.getY());
        GraphNode[] neighbours = new GraphNode[4];
        neighbours[0] = GetValue(pos.x, pos.y + 1);
        neighbours[1] = GetValue(pos.x + 1, pos.y);
        neighbours[2] = GetValue(pos.x, pos.y - 1);
        neighbours[3] = GetValue(pos.x - 1, pos.y);
        return neighbours;
    }
}
