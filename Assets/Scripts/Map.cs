using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public enum EnvironmentType
{
    Ground,
    Water,
    Shelter,
    // Bridge
}

public class Map : MonoBehaviour
{
    [SerializeField] private int width = 50;
    [SerializeField] private int height = 100;

    public EnvironmentType[,] grid;
    public Tilemap map;
    public Tilemap obstacleMap;
    public Tile[] floorTiles;
    public Tile waterTile;


    [Header("Obstacle Settings")]

    // obstacles
    public int probabilityObstaclesGeneral = 6;
    public Tile[] pillars;
    public int probabilityPillar = 20;
    public Tile[] bushes;
    public int probabilityBushes = 30;
    public Tile[] accessoirs;
    public int probabilityAccessoirs = 50;

    [Header("Standard Water Tiles")]

    public Tile northTile;
    public Tile northEastTile;
    public Tile eastTile;
    public Tile southEastTile;
    public Tile southTile;
    public Tile southWestTile;
    public Tile westTile;
    public Tile northWestTile;

    [Header("Special Water Tiles")]

    public Tile fillerNorthEast;
    public Tile fillerSoutEast;
    public Tile fillerSoutWest;
    public Tile fillerNorthWest;

    public Tile northEastAndSouthWest;
    public Tile northWestAndSouthEast;

    [Header("Other Map properies")]

    public System.Random ran = new System.Random();

    public GameObject diamondPrefab;

    private float noise_density = 50;
    private int iterations = 3; // low für viele Abschnitt, high für größere Höhlen

    void Awake()
    {
        map = GameObject.Find("Ground").GetComponent<Tilemap>();
        obstacleMap = GameObject.Find("Obstacles").GetComponent<Tilemap>();
    }

    public void GenerateMap(List<Hort> horts)
    {
        grid = GenerateNoiseGrid(noise_density);
        ApplyCellularAutomaton(grid, iterations);
        RemoveSingleTiles(grid);
        RemoveSingleTiles(grid);
        DrawTilemap(grid, map, floorTiles, waterTile);
        foreach (Hort hort in horts)
        {
            PlaceHort(hort);
        }
        PlaceObstacles();
        PlaceItems(map);
    }

    // checks if there is water or an obstacles on the given position
    private Boolean TileIsFree(int x, int y)
    {
        return grid[x, y] == EnvironmentType.Ground && obstacleMap.GetTile(new Vector3Int(x, y, 0)) == null;
    }

    public Vector2[] GetSections(int count)
    {
        Vector2[] sections = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            float countAsFloat = (float)count;
            sections[i] = new Vector2(width * (i / countAsFloat), width * ((i + 1) / countAsFloat));
        }
        return sections;
    }

    EnvironmentType[,] GenerateNoiseGrid(float density)
    {
        EnvironmentType[,] numbers = new EnvironmentType[width, height];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int random = ran.Next(1, 100);
                if (random > density)
                {
                    numbers[j, i] = EnvironmentType.Ground;
                }
                else
                {
                    numbers[j, i] = EnvironmentType.Water;
                }
            }
        }
        return numbers;
    }

    void ApplyCellularAutomaton(EnvironmentType[,] cells, int iterations)
    {
        int i = 0;
        while (i < iterations)
        {
            EnvironmentType[,] tempCells = cells.Clone() as EnvironmentType[,];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int neighbor_wall_count = 0;
                    //go through neigbors
                    for (int j = y - 1; j <= y + 1; j++)
                    {
                        for (int k = x - 1; k <= x + 1; k++)
                        {
                            //check if in bounds of array if not just assume its -
                            if (j >= 0 && j < cells.GetLength(1) && k >= 0 && k < cells.GetLength(0))
                            {
                                //check if this is not the coordinate whose neighbors we are checking
                                if (!(j == y && k == x))
                                {
                                    //check if neighbor is a - and if so add neighbor_wall_count
                                    if (tempCells[k, j] == EnvironmentType.Water)
                                    {
                                        neighbor_wall_count++;
                                    }
                                }
                            }
                            else
                            {
                                neighbor_wall_count++;
                            }
                        }
                    }
                    //if there are more than 4 neighbors that are - make the coordinate a - and if not make it +
                    if (neighbor_wall_count > 4)
                    {
                        cells[x, y] = EnvironmentType.Water;
                    }
                    else
                    {
                        cells[x, y] = EnvironmentType.Ground;
                    }
                }
            }
            i++;
        }
    }

    void DrawTilemap(EnvironmentType[,] cells, Tilemap tilemap, Tile[] floorTiles, Tile waterTile)
    {
        tilemap.ClearAllTiles();
        for (int x = 0; x < cells.GetUpperBound(0); x++)
        {
            for (int y = 0; y < cells.GetUpperBound(1); y++)
            {
                if (cells[x, y] == EnvironmentType.Ground)
                {
                    int random = ran.Next(0, floorTiles.GetLength(0) - 1);
                    tilemap.SetTile(new Vector3Int(x, y, 0), floorTiles[random]); // Floor
                }
                else
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), GetWaterTile(x, y, cells)); // Water
                }
            }
        }
    }

    void RemoveSingleTiles(EnvironmentType[,] grid)
    {

        for (int x = 0; x < grid.GetUpperBound(0); x++)
        {
            for (int y = 0; y < grid.GetUpperBound(1); y++)
            {
                if (grid[x, y] == EnvironmentType.Water)
                {
                    EnvironmentType[] directions = GetNeighbourTypes(x, y, grid);
                    if (
                        (directions[(int)Direction.NORTH] == EnvironmentType.Ground && directions[(int)Direction.SOUTH] == EnvironmentType.Ground)
                        ||
                        (directions[(int)Direction.EAST] == EnvironmentType.Ground && directions[(int)Direction.WEST] == EnvironmentType.Ground)
                    )
                    {
                        grid[x, y] = EnvironmentType.Ground;
                    }
                }
            }
        }

    }


    public enum Direction : int
    {
        NORTH = 0,
        NORTH_EAST = 1,
        EAST = 2,
        SOUTH_EAST = 3,
        SOUTH = 4,
        SOUTH_WEST = 5,
        WEST = 6,
        NORTH_WEST = 7,
    }

    int NORTH = 1, EAST = 1;
    int SOUTH = -1, WEST = -1;

    EnvironmentType[] GetNeighbourTypes(int x, int y, EnvironmentType[,] grid)
    {
        EnvironmentType[] directions = new EnvironmentType[8];

        int width = grid.GetUpperBound(0);
        int height = grid.GetUpperBound(1);

        // get tile on the given position, with out of bounds check (alternative is water tile)
        // noth
        if (y + NORTH > height) directions[(int)Direction.NORTH] = EnvironmentType.Water;
        else directions[(int)Direction.NORTH] = grid[x, y + NORTH];

        // north east
        if (y + NORTH > height || x + EAST > width) directions[(int)Direction.NORTH_EAST] = EnvironmentType.Water;
        else directions[(int)Direction.NORTH_EAST] = grid[x + EAST, y + NORTH];

        // east
        if (x + EAST > width) directions[(int)Direction.EAST] = EnvironmentType.Water;
        else directions[(int)Direction.EAST] = grid[x + EAST, y];

        // south east 
        if (y + SOUTH < 0 || x + EAST > width) directions[(int)Direction.SOUTH_EAST] = EnvironmentType.Water;
        else directions[(int)Direction.SOUTH_EAST] = grid[x + EAST, y + SOUTH];

        // south
        if (y + SOUTH < 0) directions[(int)Direction.SOUTH] = EnvironmentType.Water;
        else directions[(int)Direction.SOUTH] = grid[x, y + SOUTH];

        // south west
        if (x + WEST < 0 || y + SOUTH < 0) directions[(int)Direction.SOUTH_WEST] = EnvironmentType.Water;
        else directions[(int)Direction.SOUTH_WEST] = grid[x + WEST, y + SOUTH];

        // west
        if (x + WEST < 0) directions[(int)Direction.WEST] = EnvironmentType.Water;
        else directions[(int)Direction.WEST] = grid[x + WEST, y];

        // north west
        if (x + WEST < 0 || y + NORTH > height) directions[(int)Direction.NORTH_WEST] = EnvironmentType.Water;
        else directions[(int)Direction.NORTH_WEST] = grid[x + WEST, y + NORTH];

        return directions;
    }


    Tile GetWaterTile(int x, int y, EnvironmentType[,] cells)
    {

        EnvironmentType[] directions = GetNeighbourTypes(x, y, cells);

        // ~~~~~~~~~ return correct tiles ~~~~~~~~ //
        Boolean hasNorthGroud = directions[(int)Direction.NORTH] == EnvironmentType.Ground,
                hasNorthEastGroud = directions[(int)Direction.NORTH_EAST] == EnvironmentType.Ground,
                hasEastGroud = directions[(int)Direction.EAST] == EnvironmentType.Ground,
                hasSouthEastGroud = directions[(int)Direction.SOUTH_EAST] == EnvironmentType.Ground,
                hasSouthGroud = directions[(int)Direction.SOUTH] == EnvironmentType.Ground,
                hasSouthWestGroud = directions[(int)Direction.SOUTH_WEST] == EnvironmentType.Ground,
                hasWestGroud = directions[(int)Direction.WEST] == EnvironmentType.Ground,
                hasNorthWestGroud = directions[(int)Direction.NORTH_WEST] == EnvironmentType.Ground;

        // handle edges (G = Ground, # = Water) like this:
        if (!hasNorthGroud && !hasEastGroud && !hasWestGroud && !hasSouthGroud)
        {
            // ###
            // ###
            // ###
            if (!hasNorthEastGroud && !hasSouthEastGroud && !hasNorthWestGroud && !hasSouthWestGroud) return waterTile;

            // ##G
            // ###
            // G##
            // ground only in northwest and southeast
            if (!hasNorthEastGroud && !hasSouthWestGroud && hasNorthWestGroud && hasSouthEastGroud) return northWestAndSouthEast;
            // ground only in northeast and southwest
            if (!hasNorthWestGroud && !hasSouthEastGroud && hasNorthEastGroud && hasSouthWestGroud) return northEastAndSouthWest;

            // ##G
            // ###
            // ###
            // ground only on northeast
            if (hasNorthEastGroud) return fillerNorthEast;
            // ground only on southeast
            if (hasNorthWestGroud) return fillerNorthWest;
            // ground only on southhwest
            if (hasSouthEastGroud) return fillerSoutEast;
            // ground only on northwest
            if (hasSouthWestGroud) return fillerSoutWest;
        }

        // Standard cases
        if (hasNorthGroud)
        {
            if (hasEastGroud) return northEastTile;
            if (hasWestGroud) return northWestTile;
            return northTile;
        }
        if (hasEastGroud)
        {
            if (hasNorthGroud) return northEastTile;
            if (hasSouthGroud) return southEastTile;
            return eastTile;
        }
        if (hasSouthGroud)
        {
            if (hasEastGroud) return southEastTile;
            if (hasWestGroud) return southWestTile;
            return southTile;
        }
        if (hasWestGroud)
        {
            if (hasNorthGroud) return northWestTile;
            if (hasSouthGroud) return southWestTile;
            return westTile;
        }

        return waterTile;
    }

    void PlaceItems(Tilemap map)
    {
        BoundsInt bounds = map.cellBounds;
        GameObject diamondParent = new GameObject("Diamonds");

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                if (TileIsFree(x, y) && ran.Next(1, 100) < 20)
                {
                    // move by 0.5f to center diamond in a tile
                    GameObject item = Instantiate(diamondPrefab, new Vector3(((float)x) + 0.5f, ((float)y) + 0.5f, 0), Quaternion.identity);
                    item.transform.parent = diamondParent.transform;
                }
            }
        }
    }

    public void PlaceCharacter(CharacterBase character)
    {
        byte team = character.GetTeamNumber();
        Vector2[] sections = GetSections(2);
        int randomX, randomY;

        // upsi
        while (true)
        {
            randomY = ran.Next(0, height);
            if (team == 0)
            {
                randomX = ran.Next((int)sections[0].x, (int)sections[0].y);
            }
            else
            {
                randomX = ran.Next((int)sections[1].x, (int)sections[1].y);
            }
            // while randomX nicht auf einer wasser tile und kein obstacle auf dieser position
            if (TileIsFree(randomX, randomY)) break;
        }

        character.transform.position = new Vector3(randomX + 0.5f, randomY + 0.5f, 0);
    }


    public void PlaceHort(Hort hort)
    {
        Vector2[] sections = GetSections(8);
        int randomY = ran.Next(0 + (int)hort.transform.localScale.y, height - (int)hort.transform.localScale.y);
        int randomX = 0;
        switch (hort.team)
        {
            case 0:
                randomX = ran.Next((int)sections[1].x, (int)sections[1].y);
                break;
            case 1:
                randomX = ran.Next((int)sections[6].x, (int)sections[6].y);
                break;

            default:
                Debug.LogError("given hort has an invalid team number!");
                break;
        }

        hort.transform.position = new Vector3(randomX + 0.5f, randomY + 0.5f, 0);
        // Debug.Log(hort.transform.localScale);

        int hortX = (int)hort.transform.position.x;
        int hortY = (int)hort.transform.position.y;

        int hortWidth = (int)hort.transform.localScale.x;
        int hortHeight = (int)hort.transform.localScale.y;

        int startPositionX = (int)Math.Ceiling(hortX - (hortWidth / 2f));
        int startPositionY = (int)Math.Ceiling(hortY - (hortHeight / 2f));
        Debug.Log(startPositionX);
        int endPositionX = (int)Math.Ceiling(hortX + (hortWidth / 2f));
        int endPositionY = (int)Math.Ceiling(hortY + (hortHeight / 2f));
        Debug.Log(endPositionX);

        for (int x = startPositionX; x < endPositionX; x++)
        {
            for (int y = startPositionY; y < endPositionY; y++)
            {
                grid[x, y] = EnvironmentType.Shelter;
                // obstacleMap.SetTile(new Vector3Int(x, y), waterTile); //unkomment to visualize hort in th obstacles map
            }
        }
    }

    // place random obstacles on the obstacle Map
    public void PlaceObstacles()
    {
        obstacleMap.ClearAllTiles();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // check if position is water 
                if (TileIsFree(x, y) && ran.Next(0, 100) < 6)
                {
                    int randomValue = ran.Next(0, probabilityBushes + probabilityPillar + probabilityAccessoirs);
                    if (randomValue < probabilityPillar)
                    {
                        obstacleMap.SetTile(new Vector3Int(x, y, 0), pillars[ran.Next(0, pillars.Length)]);
                    }
                    else if (randomValue < probabilityBushes + probabilityPillar)
                    {
                        obstacleMap.SetTile(new Vector3Int(x, y, 0), bushes[ran.Next(0, bushes.Length)]);
                    }
                    else if (randomValue < probabilityBushes + probabilityPillar + probabilityAccessoirs)
                    {
                        obstacleMap.SetTile(new Vector3Int(x, y, 0), accessoirs[ran.Next(0, accessoirs.Length)]);

                    }
                }
            }
        }
    }

}
