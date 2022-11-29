using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    public Tile floorTile;
    public Tile waterTile;
    public Tile tree;
    public System.Random ran = new System.Random();

    public GameObject diamondPrefab;

    private float noise_density = 50;
    private int iterations = 3; // low für viele Abschnitt, high für größere Höhlen

    void Awake()
    {
        map = GameObject.Find("Ground").GetComponent<Tilemap>();
        obstacleMap = GameObject.Find("Obstacles").GetComponent<Tilemap>();
        grid = GenerateNoiseGrid(noise_density);
        Tile[] tiles = { floorTile, waterTile };

        ApplyCellularAutomaton(grid, iterations);
        DrawTilemap(grid, map, tiles);
        // PlaceObstacles(map);
        PlaceItems(map);
        PlaceObstacles();

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

    void DrawTilemap(EnvironmentType[,] cells, Tilemap tilemap, Tile[] tiles)
    {
        tilemap.ClearAllTiles();
        for (int x = 0; x < cells.GetUpperBound(0); x++)
        {
            for (int y = 0; y < cells.GetUpperBound(1); y++)
            {
                if (cells[x, y] == EnvironmentType.Ground)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tiles[0]); // Floor
                }
                else
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tiles[1]); // Water
                    // TODO set boundary for not walking into water
                }
            }
        }
    }

    void PlaceItems(Tilemap map)
    {
        BoundsInt bounds = map.cellBounds;
        TileBase[] allTiles = map.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile.name == floorTile.name)
                {
                    int random = ran.Next(1, 100);
                    if (random < 20)
                    {
                        // move by 0.5f to center diamond in a tile
                        GameObject item = Instantiate(diamondPrefab, new Vector3(((float)x) + 0.5f, ((float)y) + 0.5f, 0), Quaternion.identity);
                    }
                }
            }
        }
    }

    public void PlacePlayer(Player player)
    {
        byte team = player.getTeamNumber();
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
            // while randomX nicht auf einer wasser tile
            if (grid[randomX, randomY] == (int)EnvironmentType.Ground) break;
        }

        player.transform.position = new Vector3(randomX + 0.5f, randomY + 0.5f, 0);
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

        // TODO: set tiles the hort uses to EnvironmentTiles.Shelter
        // int hortHeight = 
        // int hortWidth = 
        // for (int x = 0; x < map.GetUpperBound(0); x++)
        // {
        //     for (int y = 0; y < map.GetUpperBound(1); y++)
        //     {
        //         if (map[x, y] == 0)
        //         {
        //             tilemap.SetTile(new Vector3Int(x, y, 0), tiles[0]); // Floor
        //         }
        //     }
        // }
    }

    // TODO: place random obstacles on the obstacle Map
    public void PlaceObstacles()
    {
        obstacleMap.ClearAllTiles();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (ran.Next(0, 100) < 20)
                {
                    obstacleMap.SetTile(new Vector3Int(x, y, 0), tree);
                }
            }
        }
    }

}
