using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// This class moves the bot according to the Interaction ID that is set in the BotBehavior class
/// </summary>
public class BotMovement : MonoBehaviour
{
    [HideInInspector]
    public Transform goal;

    [SerializeField]
    private float shootRange = 20f;

    [SerializeField]
    private float botSpeed = 3f;

    private BotBehavior behavior;

    private GameObject player;

    private Tilemap tilemap;

    private Pathfinding pathfinding;

    private List<GraphNode> path;

    private Graph graph;

    private int currentIndex;
    
    // Start is called before the first frame update
    void Start()
    {
        behavior = GetComponent<BotBehavior>();
        
        player = GameObject.FindWithTag("Player");
        
        tilemap = GameObject.Find("Obstacles").GetComponent<Tilemap>();
        graph = new Graph(tilemap, true);
    }

    private void Update()
    {
        // if the Interaction ID was changed stop everything and start new interaction
        if (behavior.GetChangedInteractionID())
        {
            Debug.Log("Interaction changed to " + behavior.GetInteractionID().ToString());

            path = null;
            graph.ResetGraph();
            pathfinding = new Pathfinding(graph);
            StopAllCoroutines();

            StartInteraction();

            behavior.SetChangedInteractionID(false);
        }
    }

    private void StartInteraction()
    {
        switch (behavior.GetInteractionID())
        {
            case InteractionID.Opponent:
                Debug.Log("Start interaction with opponent");
                StartCoroutine(FollowOpponent());
                break;
            case InteractionID.Teammate:
                Debug.Log("Start interaction with teammate");
                break;
            case InteractionID.OpponentBubble:
                Debug.Log("Start interaction with opponent bubble");
                break;
            case InteractionID.Diamond:
                Debug.Log("Start interaction with diamond");
                break;
            case InteractionID.Hort:
                Debug.Log("Start interaction with hort");
                break;
            case InteractionID.Item:
                Debug.Log("Start interaction with item");
                break;
            case InteractionID.None:
                Debug.Log("Start interaction with nothing");
                break;
        }
    }
    
    IEnumerator FollowOpponent()
    {
        InvokeRepeating("FollowPlayer", 1.0f, 0.5f);
        while (true)
        {
            float distToPlayer = GetEuclideanDistance(transform.position, goal.position);

            if (path != null)
            {
                Vector3 nextNode = pathfinding.GetGraph().GetWorldPosition(path[currentIndex].getX(), path[currentIndex].getY());
                float distNextNode = GetEuclideanDistance(transform.position, nextNode);
                if (distNextNode <= 5f && currentIndex < path.Count - 1)
                {
                    currentIndex++;
                }

                if (distToPlayer <= shootRange)
                {
                    Debug.Log("Shoot Bubble!");
                    CancelInvoke();
                    path = null;
                }

                transform.position = Vector3.MoveTowards(transform.position, nextNode, botSpeed * Time.deltaTime);
            }
            else if (distToPlayer >= (shootRange + 5f))
            {
                InvokeRepeating("FollowPlayer", 0.1f, 0.5f);
            }

            yield return new WaitForSeconds(0.01f);
        }
    }
    private void FollowPlayer()
    {
        path = pathfinding.FindPath(transform.position, goal.position);
        currentIndex = 0;
    }


    private float GetEuclideanDistance(Vector3 start, Vector3 end)
    {
        return Mathf.Pow(
            Mathf.Pow(end.x - start.x, 2) +
            Mathf.Pow(end.y - start.y, 2) +
            Mathf.Pow(end.z - start.z, 2), 2);
    }

    private Vector3 GetPosition()
    {
        return transform.position;
    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = currentIndex; i < path.Count; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(graph.GetWorldPosition(path[i]), Vector3.one * 0.5f);
                if (i == currentIndex)
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
}

//float distToPlayer = GetEuclideanDistance(transform.position, player.transform.position);

//if (path != null)
//{
//    Vector3 nextNode = pathfinding.GetGraph().GetWorldPosition(path[currentIndex].getX(), path[currentIndex].getY());
//    float distNextNode = GetEuclideanDistance(transform.position, nextNode);
//    if (distNextNode <= 5f && currentIndex < path.Count - 1)
//    {
//        currentIndex++;
//    }

//    if (distToPlayer <= shootRange)
//    {
//        Debug.Log("Shoot Player");
//        path = null;
//        break;
//    }

//    transform.position = Vector3.MoveTowards(transform.position, nextNode, botSpeed * Time.deltaTime);
//} 
//if (distToPlayer >= (shootRange + 5f))
//{
//    path = pathfinding.FindPath(transform.position, player.transform.position);
//    currentIndex = 0;
//    Debug.Log("new path...");
//}
