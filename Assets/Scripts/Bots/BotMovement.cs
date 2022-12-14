using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

/// <summary>
/// This class moves the bot according to the Interaction ID that is set in the BotBehavior class
/// </summary>
public class BotMovement : NetworkBehaviour
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

    private GameObject freeTileAroundHort;

    void Start()
    {
        bot = GetComponent<Bot>();
        directionIndicator = transform.Find("Triangle");

        // object holder for the transform of the goal if the interactionId is hort
        freeTileAroundHort = new GameObject();
        freeTileAroundHort.hideFlags = HideFlags.HideInHierarchy;
    }

    private void Update()
    {
        // return if not server
        if (!isServer) return;

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
            bot.ResetBot(CharacterBase.BUBBLE_BREAKOUT_TIME);
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
                Debug.Log("Start follow opponent");
                StartCoroutine(FollowOpponent());
                break;
            case InteractionID.Teammate:
                Debug.Log("Start follow teammate");
                StartCoroutine(FollowGoal());
                break;
            case InteractionID.OpponentBubble:
                Debug.Log("Start follow opponent bubble");
                // TODO: implement
                break;
            case InteractionID.Diamond:
                Debug.Log("Start follow diamond");
                StartCoroutine(FollowGoal());
                break;
            case InteractionID.Hort:
                Debug.Log("Start follow hort");
                Transform hortGoal = GetFreeTileAroundHort(goal.position);
                SetGoal(hortGoal);
                StartCoroutine(FollowGoal()); // TODO: path berechnung hat nicht für hort geklappt
                break;
            case InteractionID.Item:
                Debug.Log("Start follow item");
                StartCoroutine(FollowGoal());
                break;
            case InteractionID.None:
                Debug.Log("Start follow with nothing");
                break;
        }
    }

    IEnumerator FollowGoal()
    {
        InvokeRepeating("CalculatePathToGoal", 1.0f, 0.5f);
        while (goal != null)
        {
            float distToGoal = GetEuclideanDistance(transform.position, goal.position);

            if (path != null)
            {
                Vector3 nextNode = pathfinding.GetGraph().GetWorldPosition((int)path[currentIndex].GetX(), (int)path[currentIndex].GetY());
                float distNextNode = GetEuclideanDistance(transform.position, nextNode);
                if (distNextNode <= 0.01f && currentIndex < path.Count - 1)
                {
                    currentIndex++;
                }

                if (distToGoal <= 0.01f)
                {
                    CancelInvoke();
                    bot.ResetBot(0f);
                    path = null;
                    Debug.Log("Bot Reached goal");
                    break;
                }
                transform.position = Vector3.MoveTowards(transform.position, nextNode, botSpeed * Time.deltaTime);
            }

            yield return new WaitForSeconds(0.001f);
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

        CharacterBase opponent = goal.gameObject.GetComponent<CharacterBase>();
        
        while (true)
        {
            float distToPlayer = GetEuclideanDistance(transform.position, goal.position);
            
            if (path != null)
            {
                Vector3 nextNode = pathfinding.GetGraph().GetWorldPosition((int) path[currentIndex].GetX(), (int) path[currentIndex].GetY());
                float distNextNode = GetEuclideanDistance(transform.position, nextNode);
                if (distNextNode <= 0.01f && currentIndex < path.Count - 1)
                {
                    currentIndex++;
                }

                if (distToPlayer <= shootRange) // TODO: check if player is captured, if so find new goal
                {
                    GetComponent<Shooting>().ShootBubble();
                }
                transform.position = Vector3.MoveTowards(transform.position, nextNode, botSpeed * Time.deltaTime);
            }
            else if (distToPlayer >= (shootRange + 5f))
            {
                InvokeRepeating("CalculatePathToGoal", 0.1f, 0.5f);
            }

            if (opponent.GetIsCaptured())
            {
                CancelInvoke();
                StopAllCoroutines();
                bot.ResetBot(0f);
                path = null;
                Debug.Log("Opponent captured");
                break;
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

    private Transform GetFreeTileAroundHort(Vector2 hortCenter)
    {
        Hort hort = bot.GetHort().GetComponent<Hort>();
        Map map = GameObject.Find("Map").GetComponent<Map>();
        
        int x = (int)hortCenter.x;
        int y = (int)hortCenter.y;
        int xMin = x - (Hort.scale / 2 + 1);
        int xMax = x + (Hort.scale / 2 + 1);
        int yMin = y - (Hort.scale / 2 + 1);
        int yMax = y + (Hort.scale / 2 + 1);

        List<GraphNode> nodesAroundHort = new List<GraphNode>();
        for (int i = xMin; i <= xMax; i++)
        {
            for (int j = yMin; j <= yMax; j++)
            {
                nodesAroundHort.Add(graph.GetNode(i, j));
            }
        }

        // order nodes by distance to bot
        nodesAroundHort = nodesAroundHort.OrderBy(node => Vector3.Distance(transform.position, graph.GetWorldPosition(node.GetX(), node.GetY()))).ToList();

        // find closest node to bot that is free
        foreach (GraphNode node in nodesAroundHort)
        {
            if (map.TileIsFree(node.GetX(), node.GetY()))
            {
                freeTileAroundHort.transform.position = graph.GetWorldPosition(node.GetX(), node.GetY());
                break;
            }
        }
        
        return freeTileAroundHort.transform;
    }
    
    private Vector3 GetPosition()
    {
        return transform.position;
    }
    
    public void SetGraph(Graph graph)
    {
        this.graph = graph;
    }

    public void SetGoal(Transform goal)
    {
        this.goal = goal;
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
