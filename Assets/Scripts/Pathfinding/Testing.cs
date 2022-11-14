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
        Tilemap tilemap = GameObject.Find("Obstacles").GetComponent<Tilemap>();
        graph = new Graph(tilemap, true);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = UtilsClass.GetMouseWorldPosition();
            int value = graph.GetValue(mousePos);
            graph.SetValue(mousePos, value + 1);
        }
    }
}
