using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BotMovement : MonoBehaviour
{
    private Graph graph;
    
    private Pathfinding pathfinding;

    private List<GraphNode> path = new List<GraphNode>();

    private int pathIndex = 0;

    private float speed = 3f;

    private Rigidbody2D rb;

    private void Start()
    {
        Tilemap tilemap = GameObject.Find("Obstacles").GetComponent<Tilemap>();
        // create a new graph with the tilemap
        graph = new Graph(tilemap, true);
        // create a new pathfinding object with the graph
        pathfinding = new Pathfinding(graph);
        rb = GetComponent<Rigidbody2D>();

        Vector3 playerPos = GameObject.Find("Player").transform.position;

        AddNewPath(playerPos);

        //List<GraphNode> path = pathfinding.FindPath(new Vector3(0, 0, 0), new Vector3(5, 5, 0));
        //foreach (GraphNode node in path)
        //{
        //    //graph.HighlightNode(node);
        //    graph.SetText(node.getX(), node.getY(), "");
        //    Debug.Log("path - " + node);
        //}
    }

    private void Update()
    {
        /*if(path.Count != 0 && pathIndex < path.Count)
        {
            GraphNode nextNode = path[pathIndex];
            Vector2 nextPosition = nextNode.getPosition();
            transform.position = Vector2.MoveTowards(transform.position, nextPosition, speed * Time.deltaTime);
            pathIndex++;
        }*/
    }

    private Vector3 GetPosition()
    {
        return transform.position;
    }

    private void Move()
    {
        if (pathIndex < path.Count)
        {
            Vector3 targetPosition = graph.GetWorldPosition(path[pathIndex]);
            Vector3 direction = (targetPosition - GetPosition()).normalized;
            transform.position += direction * speed * Time.deltaTime;
            if (Vector3.Distance(GetPosition(), targetPosition) <= 0.1f)
            {
                pathIndex++;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = pathIndex; i < path.Count; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(graph.GetWorldPosition(path[i]), Vector3.one * 0.5f);
                if (i == pathIndex)
                {
                    Gizmos.DrawLine(GetPosition(), graph.GetWorldPosition(path[i]));
                }
                else
                {
                    Gizmos.DrawLine(graph.GetWorldPosition(path[i - 1]), graph.GetWorldPosition(path[i]));
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="goal"></param>
    /// <returns></returns>
    private void AddNewPath(Vector3 goal)
    {
        Vector3 currentPos = GetPosition();
        List<GraphNode> newPath = pathfinding.FindPath(currentPos, goal);
        foreach(GraphNode node in newPath)
        {
            path.Add(node);
        }
    }
}
