using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Testing : MonoBehaviour
{
    private Graph<GraphNode> graph;

    // Start is called before the first frame update
    void Start()
    {
        Tilemap tilemap = GameObject.Find("Obstacles").GetComponent<Tilemap>();
        Pathfinding pathfinding = new Pathfinding(tilemap);
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = UtilsClass.GetMouseWorldPosition();
            //var value = graph.GetValue(mousePos);
            //graph.SetValue(mousePos, value);
        }
    }
}
