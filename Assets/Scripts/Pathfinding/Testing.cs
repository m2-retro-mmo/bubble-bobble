using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Testing : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Tilemap tilemap = GameObject.Find("Obstacles").GetComponent<Tilemap>();
        Pathfinding pathfinding = new Pathfinding(tilemap);
        List<GraphNode> path = pathfinding.FindPath(new Vector3(0, 0, 0), new Vector3(5, 5, 0));
        foreach (GraphNode node in path)
        {
            Debug.Log("path - " + node);
        }
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = UtilsClass.GetMouseWorldPosition();
            //var value = graph.GetNode(mousePos);
            //graph.SetNode(mousePos, value);
        }
    }
}
