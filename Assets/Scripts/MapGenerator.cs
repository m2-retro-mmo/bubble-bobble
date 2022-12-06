using UnityEngine;
using System;
using System.Linq;

public enum FloorType : int
{
    Ground1,
    Ground2,
    Ground3,
    Ground4,
    Water,
    Shelter,
}

public static class FloorTypeMethods
{
    private static int[] groundRange = new int[2] { (int)FloorType.Ground1, (int)FloorType.Ground4 };

    public static bool IsGround(this FloorType type)
    {
        return groundRange[0] <= (int)type && (int)type <= groundRange[1];
    }

    public static FloorType GetRandomGround()
    {
        return (FloorType)UnityEngine.Random.Range(groundRange[0], groundRange[1] + 1);
    }

    public static int GetGroundIndex(this FloorType type)
    {
        Debug.Assert(type.IsGround());
        return (int)type - groundRange[0];
    }
}

public enum ObstacleType : int
{
    None,
    Pillar1,
    Pillar2,
    Bush1,
    Bush2,
    Decoration1,
    Decoration2,
    Decoration3,
}

public static class ObstacleTypeMethods
{
    private static int[] pillarRange = new int[2] { (int)ObstacleType.Pillar1, (int)ObstacleType.Pillar2 };
    private static int[] bushRange = new int[2] { (int)ObstacleType.Bush1, (int)ObstacleType.Bush2 };
    private static int[] decorationRange = new int[2] { (int)ObstacleType.Decoration1, (int)ObstacleType.Decoration3 };

    public static bool IsPillar(this ObstacleType type)
    {
        return pillarRange[0] <= (int)type && (int)type <= pillarRange[1];
    }
    public static bool IsBush(this ObstacleType type)
    {
        return bushRange[0] <= (int)type && (int)type <= bushRange[1];
    }
    public static bool IsDecoration(this ObstacleType type)
    {
        return decorationRange[0] <= (int)type && (int)type <= decorationRange[1];
    }

    public static ObstacleType GetRandomPillar()
    {
        return (ObstacleType)UnityEngine.Random.Range(pillarRange[0], pillarRange[1] + 1);
    }
    public static ObstacleType GetRandomBush()
    {
        return (ObstacleType)UnityEngine.Random.Range(bushRange[0], bushRange[1] + 1);
    }
    public static ObstacleType GetRandomDecoration()
    {
        return (ObstacleType)UnityEngine.Random.Range(decorationRange[0], decorationRange[1] + 1);
    }

    public static int GetPillarIndex(this ObstacleType type)
    {
        Debug.Assert(type.IsPillar());
        return (int)type - pillarRange[0];
    }
    public static int GetBushIndex(this ObstacleType type)
    {
        Debug.Assert(type.IsBush());
        return (int)type - bushRange[0];
    }
    public static int GetDecorationIndex(this ObstacleType type)
    {
        Debug.Assert(type.IsDecoration());
        return (int)type - decorationRange[0];
    }
}

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

public struct MapData
{
    public int width;
    public int height;
    public FloorType[] floor;
    public ObstacleType[] obstacles;
    public IntVec2[] horts;

    public MapData(int width, int height)
    {
        this.width = width;
        this.height = height;
        this.floor = new FloorType[width * height];
        this.obstacles = new ObstacleType[width * height];
        this.horts = new IntVec2[2];
    }

    public FloorType GetFloor(int x, int y)
    {
        return floor[x + y * width];
    }

    public void SetFloor(int x, int y, FloorType type)
    {
        floor[x + y * width] = type;
    }

    public ObstacleType GetObstacle(int x, int y)
    {
        return obstacles[x + y * width];
    }

    public void SetObstacle(int x, int y, ObstacleType type)
    {
        obstacles[x + y * width] = type;
    }

    public bool IsTileFree(int x, int y)
    {
        return GetFloor(x, y).IsGround() && GetObstacle(x, y) == ObstacleType.None;
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
}

public class MapGenerator
{

    // static random generator
    private static System.Random ran = new System.Random();
    // TODO: density of the noise? :D
    private static float noise_density = 50;
    // low for many section, high for larger caves
    private static int iterations = 3;
    private static int hortSize = 7;

    private static int probabilityPillar = 20;
    private static int probabilityBushes = 30;
    private static int probabilityDecoration = 50;

    public static MapData GenerateMap(int teams, int width, int height)
    {
        MapData data = new MapData(width, height);

        GenerateNoiseGrid(ref data, noise_density);
        ApplyCellularAutomaton(ref data, iterations);

        foreach (int team in Enumerable.Range(0, teams))
        {
            PlaceHort(ref data, team);
        }
        PlaceObstacles(ref data);

        return data;
    }

    static void GenerateNoiseGrid(ref MapData data, float density)
    {
        for (int i = 0; i < data.height; i++)
        {
            for (int j = 0; j < data.width; j++)
            {
                int random = ran.Next(1, 100);
                if (random > density)
                {
                    data.SetFloor(j, i, FloorTypeMethods.GetRandomGround());
                }
                else
                {
                    data.SetFloor(j, i, FloorType.Water);
                }
            }
        }
    }

    static void ApplyCellularAutomaton(ref MapData data, int iterations)
    {
        int i = 0;
        while (i < iterations)
        {
            FloorType[] tempCells = data.floor.Clone() as FloorType[];
            for (int y = 0; y < data.height; y++)
            {
                for (int x = 0; x < data.width; x++)
                {
                    int neighbor_wall_count = 0;
                    //go through neigbors
                    for (int j = y - 1; j <= y + 1; j++)
                    {
                        for (int k = x - 1; k <= x + 1; k++)
                        {
                            //check if in bounds of array if not just assume its -
                            if (j >= 0 && j < data.height && k >= 0 && k < data.width)
                            {
                                //check if this is not the coordinate whose neighbors we are checking
                                if (!(j == y && k == x))
                                {
                                    //check if neighbor is a - and if so add neighbor_wall_count
                                    if (tempCells[k + j * data.width] == FloorType.Water)
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
                        data.SetFloor(x, y, FloorType.Water);
                    }
                    else
                    {
                        data.SetFloor(x, y, FloorTypeMethods.GetRandomGround());
                    }
                }
            }
            i++;
        }
    }

    static public void PlaceHort(ref MapData data, int team)
    {
        Vector2[] sections = data.GetSections(8);
        int hortY = ran.Next(0 + hortSize, data.height - hortSize);
        int hortX = 0;
        switch (team)
        {
            case 0:
                hortX = ran.Next((int)sections[1].x, (int)sections[1].y);
                break;
            case 1:
                hortX = ran.Next((int)sections[6].x, (int)sections[6].y);
                break;

            default:
                Debug.LogError("given hort has an invalid team number!");
                break;
        }

        int startPositionX = (int)Math.Ceiling(hortX - (hortSize / 2f));
        int startPositionY = (int)Math.Ceiling(hortY - (hortSize / 2f));
        int endPositionX = (int)Math.Ceiling(hortX + (hortSize / 2f));
        int endPositionY = (int)Math.Ceiling(hortY + (hortSize / 2f));

        for (int x = startPositionX; x < endPositionX; x++)
        {
            for (int y = startPositionY; y < endPositionY; y++)
            {
                data.SetFloor(x, y, FloorType.Shelter);
            }
        }
        data.horts[team] = new IntVec2(hortX, hortY);
    }

    // place random obstacles on the obstacle Map
    static public void PlaceObstacles(ref MapData data)
    {
        for (int x = 0; x < data.width; x++)
        {
            for (int y = 0; y < data.height; y++)
            {
                // check if position is water 
                if (data.IsTileFree(x, y) && ran.Next(0, 100) < 6)
                {
                    int randomValue = ran.Next(0, probabilityBushes + probabilityPillar + probabilityDecoration);
                    if (randomValue < probabilityPillar)
                    {
                        data.SetObstacle(x, y, ObstacleTypeMethods.GetRandomPillar());
                    }
                    else if (randomValue < probabilityBushes + probabilityPillar)
                    {
                        data.SetObstacle(x, y, ObstacleTypeMethods.GetRandomBush());
                    }
                    else if (randomValue < probabilityBushes + probabilityPillar + probabilityDecoration)
                    {
                        data.SetObstacle(x, y, ObstacleTypeMethods.GetRandomDecoration());
                    }
                }
            }
        }
    }
}
