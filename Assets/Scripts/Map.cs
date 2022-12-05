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

    [Header("Water Tiles")]

    public Tile northTile;
    public Tile eastTile;
    public Tile southTile;
    public Tile westTile;

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
        DrawTilemap(grid, map, floorTiles, waterTile);
        // foreach (Hort hort in horts)
        // {
        //     PlaceHort(hort);
        // }
        // PlaceObstacles();
        // PlaceItems(map);
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

                    tilemap.SetTile(new Vector3Int(x, y, 0), waterTile); // Water
                    GetWaterTile(x, y, cells);

                    // TODO set boundary for not walking into water
                }
            }
        }
    }

    int NORTH, EAST = 1;
    int SOUTH, WEST = -1;

    void GetWaterTile(int x, int y, EnvironmentType[,] cells)
    {
        EnvironmentType north, east, south, west;

        int width = cells.GetUpperBound(0);
        int height = cells.GetUpperBound(1);

        // get tile on the given position, with out of bounds check (alternative is water tile)
        // noth
        if (y + NORTH > height)
        {
            north = EnvironmentType.Water;
        }
        else
        {
            north = cells[x, y + NORTH];
        }
        // south
        if (y + SOUTH < 0)
        {
            south = EnvironmentType.Water;
        }
        else
        {
            south = cells[x, y + SOUTH];
        }
        // east
        if (x + EAST > width)
        {
            east = EnvironmentType.Water;
        }
        else
        {
            east = cells[x + EAST, y];
        }
        // west
        if (x + WEST < 0)
        {
            west = EnvironmentType.Water;
        }
        else
        {
            west = cells[x + WEST, y];
        }





        // north = cells[x, y + 1];
        // northEast = cells[];
        // East = cells[];
        // SouthEast = cells[];
        // South = cells[];
        // SouthWest = cells[];
        // West = cells[];
        // NorthWest = cells[];
    }

    // TODO: check for obstacles as well! 
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
