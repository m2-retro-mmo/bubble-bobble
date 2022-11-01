using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum EnvironmentType
{
    Ground,
    Water,
    Bridge,
    Obstacle
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
    private const float tileSize = 4;

    private GameObject root, floor, environment;
    public GameObject ball;
    public int xHalfExt = 1;
    public int zHalfExt = 1;

    public GameObject[] floorTiles;

    private int xExt, zExt;
    private Vector3 start, end, playfieldOffset;

    public System.Random ran = new System.Random();

    public MapCell[,] cells;

    void Awake()
    {
        // root = GameObject.Find("MapContainer");
        // playfieldOffset = new Vector3(-xHalfExt, 0, -zHalfExt) * tileSize;

        // xExt = 2 * xHalfExt + 1;
        // zExt = 2 * zHalfExt + 1;

        // var environmentScaleFactor = (float) Mathf.Max(xExt, zExt) / 6f;
        // environment.transform.localScale = Vector3.one * environmentScaleFactor;

        // var basePlateScaleFactor = (float) Mathf.Max(xExt, zExt) / 3f;
        // floor.transform.localScale *= basePlateScaleFactor;

        // cells = new MapCell[xExt, zExt];

        // for (int x = 0; x < xExt; x++)
        // {
        //     for (int z = 0; z < zExt; z++)
        //     {
        //         cells[x, z] = new MapCell();
        //     }
        // }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
