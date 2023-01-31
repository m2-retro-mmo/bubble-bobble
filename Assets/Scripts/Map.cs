using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using System;

public enum EnvironmentType
{
    Ground,
    Water,
    Shelter,
    Obstacle
    // Bridge
}

// Custom Integer Vec2 struct for serialization
public struct IntVec2
{
    public int x;
    public int y;

    public IntVec2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

// Struct representing all neccecary data to reproduce a map
public struct GeneratorData
{
    public int width;
    public int height;
    public int seed;
    public int hortScale;
    public int teams;
    public IntVec2[] hortLocations;
    public int probabilityObstaclesGeneral;
    public int probabilityPillar;

    public int probabilityDecorations;
    public int probabilityTrees;
    public int diamondSpawnDelay;
    public int diamondSpawnCount;
    public int diamondCount;
    public float noiseDensity;
    public int iterations;

    public GeneratorData(int s)
    {
        width = 100;
        height = 50;
        seed = s;
        hortScale = 7;
        teams = 2;
        hortLocations = HortLocations(s, teams, width, height, hortScale);
        probabilityObstaclesGeneral = 5;
        probabilityPillar = 30;
        probabilityDecorations = 40;
        probabilityTrees = 30;
        diamondSpawnDelay = 10;
        diamondSpawnCount = 30;
        diamondCount = 0;
        noiseDensity = 50;
        iterations = 3;
    }

    static private IntVec2[] HortLocations(int seed, int teams, int width, int height, int hortScale)
    {
        // initialize new local random
        var localRan = new System.Random(seed);
        var hortLocations = new IntVec2[teams];
        // iterate over teams
        for (int i = 0; i < teams; i++)
        {
            Vector2[] sections = _GetSections(width, 8);
            int randomY = localRan.Next(0 + hortScale * 2, height - hortScale * 2);
            int randomX = 0;
            switch (i)
            {
                case 0:
                    randomX = localRan.Next((int)sections[1].x, (int)sections[1].y);
                    break;
                case 1:
                    randomX = localRan.Next((int)sections[6].x, (int)sections[6].y);
                    break;

                default:
                    Debug.LogError("given hort has an invalid team number!");
                    break;
            }
            hortLocations[i] = new IntVec2(randomX, randomY);
        }
        return hortLocations;
    }

    static private Vector2[] _GetSections(int width, int count)
    {
        Vector2[] sections = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            float countAsFloat = (float)count;
            sections[i] = new Vector2(width * (i / countAsFloat), width * ((i + 1) / countAsFloat));
        }
        return sections;
    }

    public Vector2[] GetSections(int count)
    {
        return _GetSections(width, count);
    }
}

public class Map : NetworkBehaviour
{
    public Diamond diamondPrefab;
    public Hort hortPrefab;
    public GameObject[] treePrefabs;

    public EnvironmentType[,] floorEnvironment;
    public Tilemap floorTilemap;
    public Tilemap obstacleTilemap;
    public Tile[] floorTiles;
    public Tile waterTile;

    [Header("Obstacle Settings")]
    public Tile[] pillars;
    public Tile[] bushes;
    public Tile[] accessoirs;

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

    private GameObject diamondParent;
    private GameObject treeParent;

    public Boolean[,] isWalkable;

    [SyncVar(hook = nameof(OnNewMap))]
    private GeneratorData generatorData;
    private System.Random ran;

    public float characterSpawnOffsetX = 0.45f;
    public float characterSpawnOffsetY = 0.8f;

    public int spawnedDiamonds = 0;

    private int hortDeadAreaOffset = 7;

    [Server]
    public List<Hort> NewMap()
    {
        generatorData = new GeneratorData(DateTime.Now.Millisecond);
        List<Hort> horts = new List<Hort>();
        for (byte teamNumber = 0; teamNumber < generatorData.teams; teamNumber++)
        {
            Hort hort = Instantiate(hortPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            hort.init(teamNumber);
            horts.Add(hort);
            var hortLocation = generatorData.hortLocations[teamNumber];
            hort.transform.position = new Vector3(hortLocation.x + 0.5f, hortLocation.y + 0.5f, 0);
            NetworkServer.Spawn(hort.gameObject);
        }
        BuildMap();
        return (horts);
    }

    public void OnNewMap(GeneratorData oldData, GeneratorData newData)
    {
        BuildMap();
    }

    public void BuildMap()
    {
        // create fresh diamond parent array
        if (diamondParent != null && spawnedDiamonds == 0)
        {
            Destroy(diamondParent);
        }
        if (treeParent != null)
        {
            Destroy(treeParent);
        }
        diamondParent = new GameObject("Diamonds");
        treeParent = new GameObject("Trees");

        ran = new System.Random(generatorData.seed);
        GenerateNoiseGrid();
        ApplyCellularAutomaton();
        SetMapBoundries();
        RemoveWaterAroundHort();
        for (int i = 0; i < 2; i++)
        {
            RemoveSingleTiles();
        }
        DrawTilemap();
        UpdateHortEnvironment();
        PlaceObstacles();
        StartCoroutine(RandomDiamondSpawning());
        //SetIsWalkableForObstacles();
        Vector2[] path = {
            new Vector2(0, 0),
            new Vector2(0, generatorData.height),
            new Vector2(generatorData.width, generatorData.height),
            new Vector2(generatorData.width, 0)
        };
        gameObject.GetComponent<PolygonCollider2D>().SetPath(0, path);
    }

    // checks if there is water or an obstacles on the given position
    public bool TileIsFree(int x, int y)
    {
        // check if coordinates are around the hort
        for (int i = 0; i < generatorData.hortLocations.Length; i++)
        {
            IntVec2 hortLocation = generatorData.hortLocations[i];

            int hortX = hortLocation.x;
            int hortY = hortLocation.y;

            int startPositionX = (int)Math.Ceiling(hortX - (generatorData.hortScale / 2f)) - hortDeadAreaOffset;
            int startPositionY = (int)Math.Ceiling(hortY - ((generatorData.hortScale - 3) / 2f) - 1) - hortDeadAreaOffset;
            int endPositionX = (int)Math.Ceiling(hortX + (generatorData.hortScale / 2f)) + hortDeadAreaOffset;
            int endPositionY = (int)Math.Ceiling(hortY + ((generatorData.hortScale - 3) / 2f) - 1) + hortDeadAreaOffset;

            if ((x > startPositionX && x < endPositionX) && (y > startPositionY && y < endPositionY))
            {
                return false;
            }
        }

        return floorEnvironment[x, y] == EnvironmentType.Ground && obstacleTilemap.GetTile(new Vector3Int(x, y, 0)) == null;
    }

    // Step 1
    void GenerateNoiseGrid()
    {
        EnvironmentType[,] numbers = new EnvironmentType[generatorData.width, generatorData.height];

        for (int i = 0; i < generatorData.height; i++)
        {
            for (int j = 0; j < generatorData.width; j++)
            {
                int random = ran.Next(1, 100);
                if (random > generatorData.noiseDensity)
                {
                    numbers[j, i] = EnvironmentType.Ground;
                }
                else
                {
                    numbers[j, i] = EnvironmentType.Water;
                }
            }
        }
        floorEnvironment = numbers;
    }

    // Step 2
    void ApplyCellularAutomaton()
    {
        int i = 0;
        while (i < generatorData.iterations)
        {
            EnvironmentType[,] tmpGrid = floorEnvironment.Clone() as EnvironmentType[,];
            for (int y = 0; y < generatorData.height; y++)
            {
                for (int x = 0; x < generatorData.width; x++)
                {
                    int neighbor_wall_count = 0;
                    //go through neigbors
                    for (int j = y - 1; j <= y + 1; j++)
                    {
                        for (int k = x - 1; k <= x + 1; k++)
                        {
                            //check if in bounds of array if not just assume its -
                            if (j >= 0 && j < floorEnvironment.GetLength(1) && k >= 0 && k < floorEnvironment.GetLength(0))
                            {
                                //check if this is not the coordinate whose neighbors we are checking
                                if (!(j == y && k == x))
                                {
                                    //check if neighbor is a - and if so add neighbor_wall_count
                                    if (tmpGrid[k, j] == EnvironmentType.Water)
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
                        floorEnvironment[x, y] = EnvironmentType.Water;
                    }
                    else
                    {
                        floorEnvironment[x, y] = EnvironmentType.Ground;
                    }
                }
            }
            i++;
        }
    }

    // Step 3
    void SetMapBoundries()
    {
        for (int x = 0; x < generatorData.width; x++)
        {
            for (int y = 0; y < generatorData.height; y++)
            {
                if (x == 0 || y == 0 || x == generatorData.width - 1 || y == generatorData.height - 1)
                {
                    floorEnvironment[x, y] = EnvironmentType.Water;
                }
            }
        }
    }

    // Step 4
    void RemoveWaterAroundHort()
    {
        for (int i = 0; i < generatorData.hortLocations.Length; i++)
        {
            IntVec2 hortLocation = generatorData.hortLocations[i];

            int hortX = hortLocation.x;
            int hortY = hortLocation.y;

            int startPositionX = (int)Math.Ceiling(hortX - (generatorData.hortScale / 2f)) - hortDeadAreaOffset;
            int startPositionY = (int)Math.Ceiling(hortY - ((generatorData.hortScale - 3) / 2f) - 1) - hortDeadAreaOffset;
            int endPositionX = (int)Math.Ceiling(hortX + (generatorData.hortScale / 2f)) + hortDeadAreaOffset;
            int endPositionY = (int)Math.Ceiling(hortY + ((generatorData.hortScale - 3) / 2f) - 1) + hortDeadAreaOffset;

            for (int x = startPositionX; x < endPositionX; x++)
            {
                for (int y = startPositionY; y < endPositionY; y++)
                {
                    floorEnvironment[x, y] = EnvironmentType.Ground;
                }
            }
        }

    }

    // Step 5
    void RemoveSingleTiles()
    {
        for (int x = 0; x < generatorData.width; x++)
        {
            for (int y = 0; y < generatorData.height; y++)
            {
                if (floorEnvironment[x, y] == EnvironmentType.Water)
                {
                    EnvironmentType[] directions = GetNeighbourTypes(x, y, floorEnvironment);
                    if (
                        (directions[(int)Direction.NORTH] == EnvironmentType.Ground && directions[(int)Direction.SOUTH] == EnvironmentType.Ground)
                        ||
                        (directions[(int)Direction.EAST] == EnvironmentType.Ground && directions[(int)Direction.WEST] == EnvironmentType.Ground)
                        ||
                        (directions[(int)Direction.NORTH] == EnvironmentType.Ground && directions[(int)Direction.WEST] == EnvironmentType.Ground && directions[(int)Direction.SOUTH_EAST] == EnvironmentType.Ground)
                        ||
                        (directions[(int)Direction.SOUTH] == EnvironmentType.Ground && directions[(int)Direction.WEST] == EnvironmentType.Ground && directions[(int)Direction.NORTH_EAST] == EnvironmentType.Ground)
                    )
                    {
                        floorEnvironment[x, y] = EnvironmentType.Ground;
                    }
                }
            }
        }
    }

    // Step 6
    void DrawTilemap()
    {
        isWalkable = new Boolean[generatorData.width, generatorData.height];
        floorTilemap.ClearAllTiles();
        for (int x = 0; x < generatorData.width; x++)
        {
            for (int y = 0; y < generatorData.height; y++)
            {
                if (floorEnvironment[x, y] == EnvironmentType.Ground)
                {
                    int random = ran.Next(0, floorTiles.GetLength(0) - 1);
                    floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTiles[random]); // Floor
                    floorTilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0);
                    isWalkable[x, y] = true;
                }
                else
                {
                    floorTilemap.SetTile(new Vector3Int(x, y, 0), GetWaterTile(x, y, floorEnvironment)); // Water
                    floorTilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0);
                }
            }
        }
    }

    // Step 7
    public void UpdateHortEnvironment()
    {
        // iterate over generatorData.hortLocations with index
        for (int i = 0; i < generatorData.hortLocations.Length; i++)
        {
            IntVec2 hortLocation = generatorData.hortLocations[i];

            int hortX = hortLocation.x;
            int hortY = hortLocation.y;

            int startPositionX = (int)Math.Ceiling(hortX - (generatorData.hortScale / 2f));
            int startPositionY = (int)Math.Ceiling(hortY - ((generatorData.hortScale - 3) / 2f) - 1);
            //Debug.Log(startPositionX);
            int endPositionX = (int)Math.Ceiling(hortX + (generatorData.hortScale / 2f));
            int endPositionY = (int)Math.Ceiling(hortY + ((generatorData.hortScale - 3) / 2f) - 1);
            //Debug.Log(endPositionX);

            for (int x = startPositionX; x < endPositionX; x++)
            {
                for (int y = startPositionY; y < endPositionY; y++)
                {
                    floorEnvironment[x, y] = EnvironmentType.Shelter;
                    isWalkable[x, y] = false;
                }
            }
        }
    }

    // Step 8
    public void PlaceObstacles()
    {
        obstacleTilemap.ClearAllTiles();
        int counter = 0;
        for (int x = 0; x < generatorData.width; x++)
        {
            for (int y = 0; y < generatorData.height; y++)
            {
                // check if position is water 
                if (TileIsFree(x, y) && ran.Next(0, 100) < 6)
                {
                    int randomValue = ran.Next(0, generatorData.probabilityPillar + generatorData.probabilityDecorations + generatorData.probabilityTrees);
                    if (randomValue < generatorData.probabilityPillar)
                    {
                        obstacleTilemap.SetTile(new Vector3Int(x, y, 0), pillars[ran.Next(0, pillars.Length)]);
                        obstacleTilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0);
                        counter++;
                        isWalkable[x, y] = false;
                        floorEnvironment[x, y] = EnvironmentType.Obstacle;
                    }
                    else if (randomValue < generatorData.probabilityPillar + generatorData.probabilityDecorations)
                    {
                        obstacleTilemap.SetTile(new Vector3Int(x, y, 0), accessoirs[ran.Next(0, accessoirs.Length)]);
                        obstacleTilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0);
                        counter++;
                        isWalkable[x, y] = false;
                        floorEnvironment[x, y] = EnvironmentType.Obstacle;
                    }
                    else if (randomValue < generatorData.probabilityPillar + generatorData.probabilityDecorations + generatorData.probabilityTrees)
                    {
                        int randomTree = ran.Next(1, treePrefabs.Length);
                        GameObject tree = Instantiate(treePrefabs[randomTree], new Vector3(((float)x + 0.5f), ((float)y + 0.5f), 0), Quaternion.identity);
                        tree.transform.parent = treeParent.transform;
                        counter++;
                        isWalkable[x, y] = false;
                        floorEnvironment[x, y] = EnvironmentType.Obstacle;
                    }
                    else
                    {
                        Debug.Log("h");
                    }
                }
            }
        }
    }

    // Step 9 (Server only)
    [ServerCallback]
    void PlaceDiamonds(int count)
    {
        while (count > 0)
        {
            int x = ran.Next(0, generatorData.width);
            int y = ran.Next(0, generatorData.height);
            if (TileIsFree(x, y))
            {
                Diamond item = Instantiate(diamondPrefab, new Vector3(((float)x + 0.5f), ((float)y + 0.5f), 0), Quaternion.identity);
                item.transform.parent = diamondParent.transform;
                spawnedDiamonds++;
                count--;
                NetworkServer.Spawn(item.gameObject);
            }
        }
    }

    IEnumerator RandomDiamondSpawning()
    {
        while (true)
        {
            if (spawnedDiamonds < generatorData.diamondSpawnCount / 2 || spawnedDiamonds == 0)
            {
                int missingDiamondCount = generatorData.diamondSpawnCount - spawnedDiamonds;
                PlaceDiamonds(missingDiamondCount);
            }
            yield return new WaitForSeconds((float)generatorData.diamondSpawnDelay);
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

    EnvironmentType[] GetNeighbourTypes(int x, int y, EnvironmentType[,] floorEnvironment)
    {
        EnvironmentType[] directions = new EnvironmentType[8];

        int width = floorEnvironment.GetUpperBound(0);
        int height = floorEnvironment.GetUpperBound(1);

        // get tile on the given position, with out of bounds check (alternative is water tile)
        // noth
        if (y + NORTH > height) directions[(int)Direction.NORTH] = EnvironmentType.Water;
        else directions[(int)Direction.NORTH] = floorEnvironment[x, y + NORTH];

        // north east
        if (y + NORTH > height || x + EAST > width) directions[(int)Direction.NORTH_EAST] = EnvironmentType.Water;
        else directions[(int)Direction.NORTH_EAST] = floorEnvironment[x + EAST, y + NORTH];

        // east
        if (x + EAST > width) directions[(int)Direction.EAST] = EnvironmentType.Water;
        else directions[(int)Direction.EAST] = floorEnvironment[x + EAST, y];

        // south east 
        if (y + SOUTH < 0 || x + EAST > width) directions[(int)Direction.SOUTH_EAST] = EnvironmentType.Water;
        else directions[(int)Direction.SOUTH_EAST] = floorEnvironment[x + EAST, y + SOUTH];

        // south
        if (y + SOUTH < 0) directions[(int)Direction.SOUTH] = EnvironmentType.Water;
        else directions[(int)Direction.SOUTH] = floorEnvironment[x, y + SOUTH];

        // south west
        if (x + WEST < 0 || y + SOUTH < 0) directions[(int)Direction.SOUTH_WEST] = EnvironmentType.Water;
        else directions[(int)Direction.SOUTH_WEST] = floorEnvironment[x + WEST, y + SOUTH];

        // west
        if (x + WEST < 0) directions[(int)Direction.WEST] = EnvironmentType.Water;
        else directions[(int)Direction.WEST] = floorEnvironment[x + WEST, y];

        // north west
        if (x + WEST < 0 || y + NORTH > height) directions[(int)Direction.NORTH_WEST] = EnvironmentType.Water;
        else directions[(int)Direction.NORTH_WEST] = floorEnvironment[x + WEST, y + NORTH];

        return directions;
    }


    Tile GetWaterTile(int x, int y, EnvironmentType[,] cells)
    {

        EnvironmentType[] directions = GetNeighbourTypes(x, y, cells);

        // ~~~~~~~~~ return correct tiles ~~~~~~~~ //
        bool hasNorthGroud = directions[(int)Direction.NORTH] == EnvironmentType.Ground,
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
            if (hasSouthWestGroud) return northWestTile;
            if (hasSouthEastGroud) return northEastTile;
            return northTile;
        }
        if (hasEastGroud)
        {
            if (hasNorthGroud) return northEastTile;
            if (hasSouthGroud) return southEastTile;
            if (hasNorthWestGroud) return northEastTile;
            if (hasSouthWestGroud) return southEastTile;
            return eastTile;
        }
        if (hasSouthGroud)
        {
            if (hasEastGroud) return southEastTile;
            if (hasWestGroud) return southWestTile;
            if (hasNorthEastGroud) return southEastTile;
            if (hasNorthWestGroud) return southWestTile;
            return southTile;
        }
        if (hasWestGroud)
        {
            if (hasNorthGroud) return northWestTile;
            if (hasSouthGroud) return southWestTile;
            if (hasNorthEastGroud) return northWestTile;
            if (hasSouthEastGroud) return southWestTile;

            return westTile;
        }

        return waterTile;
    }

    [Server]
    public void PlaceCharacter(CharacterBase character)
    {
        byte team = character.GetTeamNumber();
        Vector2[] sections = generatorData.GetSections(2);
        int randomX, randomY;

        // upsi
        while (true)
        {
            randomY = ran.Next(0, generatorData.height);
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

        character.transform.position = new Vector3(randomX + characterSpawnOffsetX, randomY + characterSpawnOffsetY, 0);
    }

    public int GetWidth()
    {
        return this.generatorData.width;
    }

    public int GetHeight()
    {
        return this.generatorData.height;
    }

    public Boolean[,] GetIsWalkable()
    {
        return this.isWalkable;
    }

    public IntVec2 GetHortPosition(byte teamNumber)
    {
        return generatorData.hortLocations[teamNumber];
    }

    public int GetHortScale()
    {
        return generatorData.hortScale;
    }

    public void UpdateSpawnedDiamond(int count)
    {
        this.spawnedDiamonds += count;
        return;
    }
}
