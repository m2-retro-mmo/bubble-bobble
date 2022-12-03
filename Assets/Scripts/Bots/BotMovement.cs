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
    
    private float shootRange = 200f;
    
    private float botSpeed = 3f;

    private Bot bot;

    private Pathfinding pathfinding;

    private List<GraphNode> path;

    private int currentIndex;

    private Transform directionIndicator;
    
    private Graph graph;

    void Start()
    {
        bot = GetComponent<Bot>();
        directionIndicator = transform.Find("Triangle");
    }

    private void Update()
    {
        // if the Interaction ID was changed stop everything and start new interaction
        if (bot.GetChangedInteractionID())
        {
            Debug.Log("Interaction changed to " + bot.GetInteractionID().ToString());

            // reset everythin for new interaction
            path = null;
            graph.ResetGraph();
            pathfinding = new Pathfinding(graph);
            StopAllCoroutines();

            StartInteraction();

            bot.SetChangedInteractionID(false);
        }
        if(goal != null)
        {
            LookAtGoal();
        }
        
        if (bot.GetIsCaptured())
        {
            CancelInvoke();
            StopAllCoroutines();
            bot.ResetBot();
        }
    }

    /// <summary>
    /// Starts the interaction according to the Interaction ID.
    /// </summary>
    private void StartInteraction()
    {
        switch (bot.GetInteractionID())
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

    /// <summary>
    /// Follows the opponent.
    /// if opponent is in shooting range, shoot opponent.
    /// </summary>
    /// <returns>An IEnumerator.</returns>
    IEnumerator FollowOpponent()
    {
        InvokeRepeating("CalculatePathToGoal", 1.0f, 0.5f);
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
                    GetComponent<Shooting>().ShootBubble();
                    CancelInvoke();
                    path = null;
                }

                transform.position = Vector3.MoveTowards(transform.position, nextNode, botSpeed * Time.deltaTime);
            }
            else if (distToPlayer >= (shootRange + 5f))
            {
                InvokeRepeating("CalculatePathToGoal", 0.1f, 0.5f);
            }

            yield return new WaitForSeconds(0.001f);
        }
    }

    /// <summary>
    /// Calculates the path to goal and resets the path index.
    /// </summary>
    private void CalculatePathToGoal()
    {
        path = pathfinding.FindPath(transform.position, goal.position);
        currentIndex = 0;
    }


    /// <summary>
    /// Gets the euclidean distance.
    /// </summary>
    /// <param name="start">The start.</param>
    /// <param name="end">The end.</param>
    /// <returns>A float.</returns>
    private float GetEuclideanDistance(Vector3 start, Vector3 end)
    {
        return Mathf.Pow(
            Mathf.Pow(end.x - start.x, 2) +
            Mathf.Pow(end.y - start.y, 2) +
            Mathf.Pow(end.z - start.z, 2), 2);
    }

    /// <summary>
    /// rotates the direction indicator around the bot to look at the goal.
    /// </summary>
    private void LookAtGoal()
    {
        Vector3 lookDir = goal.position - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        directionIndicator.position = transform.position + lookDir.normalized * 1f;
        directionIndicator.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    private Vector3 GetPosition()
    {
        return transform.position;
    }
    
    public void SetGraph(Graph graph)
    {
        this.graph = graph;
    }

    /// <summary>
    /// draws the path in the scene view
    /// </summary>
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
