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

    public GameObject diamondPrefab;

    private float noise_density = 50;
    private int iterations = 3; // low für viele Abschnitt, high für größere Höhlen

    void Awake()
    {
        map = GameObject.Find("Ground").GetComponent<Tilemap>();
        int[,] grid = GenerateNoiseGrid(noise_density);
        Tile[] tiles = { floorTile, waterTile };
        
        apply_cellular_automaton(grid, iterations);
        DrawTilemap(grid, map, tiles);
        PlaceItems(map);
    }

    int[,] GenerateNoiseGrid(float density)
    {
        int[,] numbers = new int[width, height];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int random = ran.Next(1, 100);
                if (random > density)
                {
                    numbers[j, i] = 0; // tile.floor
                }
                else {
                    numbers[j, i] = 1; // tile.water
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
            Debug.Log(height);
            Debug.Log(cells.GetLength(1));
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
                        cells[x, y] = 1;
                    }
                    else
                    {
                        cells[x, y] = 0;
                    }
                }
            }
            i++;
        }
    }

    void DrawTilemap(int[,] map, Tilemap tilemap, Tile[] tiles)
    {
        tilemap.ClearAllTiles();
        for (int x = 0; x < map.GetUpperBound(0); x++)
        {
            for (int y = 0; y < map.GetUpperBound(1); y++)
            {
                if (map[x, y] == 0)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tiles[0]); // Floor
                }
                else 
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tiles[1]); // Water
                }
            }
        }
    }

    void PlaceItems(Tilemap map)
    {
        BoundsInt bounds = map.cellBounds;
        TileBase[] allTiles = map.GetTilesBlock(bounds);
        // TODO: save as global reference 
        //int[,] mapCopy = new int[tilemap.,];

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
}
