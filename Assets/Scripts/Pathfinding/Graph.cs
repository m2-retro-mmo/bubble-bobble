using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Graph
{
    private int width;
    private int height;
    private Tilemap tilemap;
    private BoundsInt bounds;
    private int[,] graphArray;
    private TextMesh[,] debugTextArray;

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

        graphArray = new int[width, height];
        debugTextArray = new TextMesh[width, height];

        // loop through the tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // get the tile at the current position
                TileBase tile = tiles[x + y * width];
                // if the tile is not null
                if (tile != null)
                {
                    // set the value of the tilemap array at the current position to 1
                    graphArray[x, y] = 1;
                    Debug.Log("Tilemap: " + x + ", " + y);
                }
                if (drawGraph)
                {
                    debugTextArray[x, y] =  UtilsClass.CreateWorldText(graphArray[x, y].ToString(), textParent.transform, GetWorldPosition(x, y) + new Vector3(0.5f, 0.5f), 10, Color.white, TextAnchor.MiddleCenter);
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

    public int GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return graphArray[x, y];
        }
        else
        {
            return 0;
        }
    }

    public int GetValue(Vector3 worldPosition)
    {
        Vector2Int pos = GetXY(worldPosition);
        return GetValue(pos.x, pos.y);
    }

    public void SetValue(int x, int y, int value)
    {
        if (0 <= x && x < width && 
            0 <= y && y < height)
        {
            graphArray[x, y] = value;
            debugTextArray[x, y].text = graphArray[x, y].ToString();
        }
    }

    public void SetValue(Vector3 worldPosition, int value)
    {
        Vector2Int pos = GetXY(worldPosition);
        SetValue(pos.x, pos.y, value);
    }
}
