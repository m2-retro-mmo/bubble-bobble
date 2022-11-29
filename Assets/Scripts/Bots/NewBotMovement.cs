using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NewBotMovement : MonoBehaviour
{
    private GameObject player;

    private Tilemap tilemap;

    private Pathfinding pathfinding;

    private List<GraphNode> path;

    private Graph graph;

    private int currentIndex;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        tilemap = GameObject.Find("Obstacles").GetComponent<Tilemap>();
        graph = new Graph(tilemap, true);
        pathfinding = new Pathfinding(graph);

        //Execute FollowPlayer() every 500ms, 1sec into game play
        InvokeRepeating("CalculatePathToGoal", 1.0f, 0.5f);
    }

    private void Update()
    {

        float distToPlayer = GetEuclideanDistance(transform.position, player.transform.position);

        if (path != null)
        {
            Vector3 nextNode = pathfinding.GetGraph().GetWorldPosition(path[currentIndex].getX(), path[currentIndex].getY());
            float distNextNode = GetEuclideanDistance(transform.position, nextNode);
            if (distNextNode <= 5f && currentIndex < path.Count - 1)
            {
                currentIndex++;
            }
            
            if (distToPlayer <= 20f)
            {
                CancelInvoke();
                path = null;
            }

            transform.position = Vector3.MoveTowards(transform.position, nextNode, 3f * Time.deltaTime);            
        } else if (distToPlayer >= 25f)
        {
            InvokeRepeating("CalculatePathToGoal", 0.1f, 0.5f);
        }


    }

    private float GetEuclideanDistance(Vector3 start, Vector3 end)
    {
        return Mathf.Pow(
            Mathf.Pow(end.x - start.x, 2) +
            Mathf.Pow(end.y - start.y, 2) +
            Mathf.Pow(end.z - start.z, 2), 2);
    }

    private void FollowPlayer()
    {
        path = pathfinding.FindPath(transform.position, player.transform.position);
        currentIndex = 0;
    }
}