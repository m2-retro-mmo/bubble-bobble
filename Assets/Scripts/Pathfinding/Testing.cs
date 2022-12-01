using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Testing : MonoBehaviour
{
    private Graph graph;
    
    // Start is called before the first frame update
    void Start()
    {
        Tilemap obstacle_tilemap = GameObject.Find("Obstacles").GetComponent<Tilemap>();
        Tilemap ground_tilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
        // create a new graph with the tilemap
        graph = new Graph(obstacle_tilemap, ground_tilemap, true);
        Pathfinding pathfinding = new Pathfinding(graph);
        List<GraphNode> path = pathfinding.FindPath(new Vector3(0, 0, 0), new Vector3(5, 5, 0));
        foreach (GraphNode node in path)
        {
            //graph.HighlightNode(node);
            graph.SetText(node.getX(), node.getY(), "");
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
