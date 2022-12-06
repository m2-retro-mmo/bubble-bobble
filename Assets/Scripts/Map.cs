using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;

public class Map : NetworkBehaviour
{
    [SerializeField] private int width = 25;
    [SerializeField] private int height = 25;

    public Tilemap map;
    public Tilemap obstacleMap;

    public Tile[] floorTiles;
    public Tile waterTile;
    public Tile[] pillars;
    public Tile[] bushes;
    public Tile[] accessoirs;

    public GameObject diamondPrefab;

    [SyncVar (hook = nameof(HookUpdatedTilemap))]
    private MapData mapData;

    private static System.Random ran = new System.Random();

    void Awake()
    {
        map = GameObject.Find("Ground").GetComponent<Tilemap>();
        obstacleMap = GameObject.Find("Obstacles").GetComponent<Tilemap>();
    }

    [Server]
    public void GenerateMap(List<Hort> horts)
    {
        mapData = MapGenerator.GenerateMap(horts.Count, width, height);
        foreach (Hort hort in horts)
        {
            IntVec2 pos = mapData.horts[hort.team];
            hort.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
        }
        DrawTilemap();
        PlaceItems();
    }

    void HookUpdatedTilemap(MapData oldMapData, MapData newMapData)
    {
        mapData = newMapData;
        DrawTilemap();
    }

    void DrawTilemap()
    {
        map.ClearAllTiles();
        obstacleMap.ClearAllTiles();
        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                FloorType floorType = mapData.GetFloor(x, y);
                if (floorType.IsGround())
                {
                    map.SetTile(new Vector3Int(x, y, 0),
                        floorTiles[FloorTypeMethods.GetGroundIndex(floorType)]);
                }
                else
                {
                    map.SetTile(new Vector3Int(x, y, 0), waterTile);
                }
                ObstacleType obstacleType = mapData.GetObstacle(x, y);
                if (obstacleType == ObstacleType.None)
                {
                    continue;
                }
                if (obstacleType.IsBush())
                {
                    obstacleMap.SetTile(new Vector3Int(x, y, 0),
                        bushes[ObstacleTypeMethods.GetBushIndex(obstacleType)]);
                }
                else if (obstacleType.IsPillar())
                {
                    obstacleMap.SetTile(new Vector3Int(x, y, 0),
                        pillars[ObstacleTypeMethods.GetPillarIndex(obstacleType)]);
                }
                else if (obstacleType.IsDecoration())
                {
                    obstacleMap.SetTile(new Vector3Int(x, y, 0),
                        accessoirs[ObstacleTypeMethods.GetDecorationIndex(obstacleType)]);
                }
            }
        }
    }

    // TODO: check for obstacles as well! 
    void PlaceItems()
    {
        BoundsInt bounds = map.cellBounds;
        GameObject diamondParent = new GameObject("Diamonds");

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                if (mapData.IsTileFree(x, y) && ran.Next(1, 100) < 20)
                {
                    // move by 0.5f to center diamond in a tile
                    GameObject item = Instantiate(diamondPrefab, new Vector3(((float)x) + 0.5f, ((float)y) + 0.5f, 0), Quaternion.identity);
                    item.transform.parent = diamondParent.transform;
                    NetworkServer.Spawn(item);
                }
            }
        }
    }

    public void PlaceCharacter(CharacterBase character)
    {
        byte team = character.GetTeamNumber();
        Vector2[] sections = mapData.GetSections(2);
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
            if (mapData.IsTileFree(randomX, randomY)) break;
        }

        character.transform.position = new Vector3(randomX + 0.5f, randomY + 0.5f, 0);
    }

}
