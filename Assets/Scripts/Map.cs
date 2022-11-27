using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

enum EnvironmentType
{
    Ground,
    Water,
    // Bridge,
    // Obstacle
}

public class Map : MonoBehaviour
{
    [SerializeField] private int width = 50;
    [SerializeField] private int height = 100;
    public Tilemap map;
    public Tile floorTile;
    public Tile waterTile;
    public System.Random ran = new System.Random();

    private float noise_density = 40;
    private int iterations = 1; // low für viele Abschnitt, high für größere Höhlen

    void Awake()
    {
        map = GameObject.Find("Ground").GetComponent<Tilemap>();
        int[,] grid = GenerateNoiseGrid(noise_density);
        Tile[] tiles = { floorTile, waterTile };
        
        apply_cellular_automaton(grid, iterations);
        DrawTilemap(grid, map, tiles);
    }

    int[,] GenerateNoiseGrid(float density)
    {
        int[,] numbers = new int[height, width];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int random = ran.Next(1, 100);
                Debug.Log(random);
                if (random > density)
                {
                    numbers[i,j] = 0; // tile.floor
                }
                else {
                    numbers[i,j] = 1; // tile.water
                }
            }
        }
        return numbers;
    }

    void apply_cellular_automaton(int[,] cells, int iterations)
    {
        int i = 0;
        while(i < iterations)
        {
            int[,] tempCells = cells.Clone() as int[,];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int neighbor_wall_count = 0;
                    //go through neigbors
                    for (int j = y-1; j <= y+1; j++)
                    {
                        for(int k = x-1; k <= x+1; k++)
                        {
                            //check if in bounds of array if not just assume its -
                            if (j >= 0 && j < cells.GetLength(1) && k >= 0 && k < cells.GetLength(0))
                            {
                                //check if this is not the coordinate whose neighbors we are checking
                                if (!(j == y && k == x)) {
                                    //check if neighbor is a - and if so add neighbor_wall_count
                                    if (tempCells[k, j] == 1)
                                    {
                                        neighbor_wall_count++;
                                        Debug.Log(neighbor_wall_count);
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
                        cells[y, x] = 1;
                        Debug.Log("+");
                    }
                    else
                    {
                        cells[y, x] = 0;
                        Debug.Log("-");
                    }
                }
            }
            i++;
        }
    }

    // int CheckWalls(int[,] cells, int cellJ, int cellK)
    // {
    //     // TODO: set Maximum X / Y boundary
    //     int maxX = cells.GetUpperBound(0);
    //     int maxY = cells.GetUpperBound(1);
    //     int NeighborWallCount = 0;

    //     // 1 / 8 N
    //     int posN = cells[cellJ - 1, cellK];
    //     if (cellJ - 1 >= 0 && posN == 1) { NeighborWallCount++; }
        
    //     // 2 / 8 NO
    //     int posNO = cells[cellJ - 1, cellK + 1];
    //     if (cellJ - 1 >= 0 && cellK + 1 < maxY && posNO == 1) { NeighborWallCount++; }

    //     // 3 / 8 O
    //     int posO = cells[cellJ, cellK + 1];
    //     if (cellK + 1 < maxY && posO == 1) { NeighborWallCount++; }

    //     // 4 / 8 OS
    //     int posOS = cells[cellJ + 1, cellK + 1];
    //     if (cellJ + 1 < maxX && cellK + 1 < maxY && posOS == 1) { NeighborWallCount++; }

    //     // 5 / 8 S
    //     int posS = cells[cellJ + 1, cellK];
    //     if (cellJ + 1 < maxX && posS == 1) { NeighborWallCount++; }

    //     // 6 / 8 SW
    //     int posSW = cells[cellJ + 1, cellK - 1];
    //     if (cellJ + 1 < maxX && cellK - 1 >= 0 && posSW == 1) { NeighborWallCount++; }

    //     // 7 / 8 W
    //     int posW = cells[cellJ, cellK - 1];
    //     if (cellK - 1 >= 0 && posW == 1) { NeighborWallCount++; }

    //     // 8 / 8 WN
    //     int posWN = cells[cellJ - 1, cellK - 1];
    //     if (cellJ - 1 >= 0 && cellK - 1 >= 0 && posWN == 1) { NeighborWallCount++; }
        
    //     return NeighborWallCount;
    // }

    void DrawTilemap(int[,] map, Tilemap tilemap, Tile[] tiles)
    {
        tilemap.ClearAllTiles();
        for (int x = 0; x < map.GetUpperBound(0); x++)
        {
            for (int y = 0; y < map.GetUpperBound(1); y++)
            {
                if (map[x, y] == 0)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tiles[0]);
                }
                else 
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tiles[1]);
                }
            }
        }
    }
}
