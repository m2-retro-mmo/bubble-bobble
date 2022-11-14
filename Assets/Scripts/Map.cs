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

public class MapCell
{
    private EnvironmentType type;

    public MapCell()
    {
        
    }
}

public class Map : MonoBehaviour
{
    [SerializeField] private int width = 200;
    [SerializeField] private int height = 300;
    public Tilemap map = GameObject.Find("MapContainer").GetComponent<Tilemap>();
    // public MapCell[,] cells;
    // public MapCell[,] gen_cells;
    public Tile floorTile = Tile.CreateInstance<Tile>();
    public Tile waterTile = Tile.CreateInstance<Tile>();
    public System.Random ran = new System.Random();

    private float noise_density = 60;
    private int iterations = 10; // low für viele Abschnitt, high für größere Höhlen

    void Awake()
    {
        // Step 1: Create noise grid --> matches the desired size of the final map
        // public TileBase[] tiles = { floorTile, waterTile };
        int[,] grid = generate_noise_grid(noise_density);
        
        apply_cellular_automaton(grid, iterations);
        draw_tilemap(grid, map, floorTile);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    int[,] generate_noise_grid(float density)
    {
        int[,] numbers = new int[height, width];
        int random = ran.Next(1, 100);

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
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
            int[,] tempCells = cells;
            for (int j = 0; j < height; j++)
            {

                for (int k = 0; k < width; k++)
                {
                    int NeighborWallCount = 0;

                    CheckWalls(j, k);
                    
                    if (NeighborWallCount > 4)
                    {
                        cells[j,k] = 1; // tile.water
                    }
                    else
                    {
                        cells[j,k] = 0; // tile.floor
                    }
                }
            }
            i++;
        }
    }

    int CheckWalls(int cellJ, int cellK)
    {
        // TODO: Check all neighbors if wall
        return 4;
    }

    void draw_tilemap(int[,] map, Tilemap tilemap, TileBase tile)
    {
        // var grid = GameObject.Find("MapContainer");
        tilemap.ClearAllTiles();
        for (int x = 0; x < map.GetUpperBound(0); x++)
        {
            for (int y = 0; y < map.GetUpperBound(1); y++)
            {
                if (map[x, y] == 0)
                {
                    // create tilemap floor
                    // GetTileByName()
                    // var mapTile = new GameObject("Tilemap").AddComponent<Tilemap>();
                    // mapTile.transform.SetParent(map.gameObject);
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
                else {
                    // create tilemap water
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }
}
