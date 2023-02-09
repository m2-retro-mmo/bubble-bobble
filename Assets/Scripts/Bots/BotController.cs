using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// This class controls the actions of the bot according to the Interaction ID that is set in the Bot class
/// </summary>
public class BotController : MonoBehaviour
{
    [HideInInspector]
    public Transform goal;

    private GameManager gameManager;

    private bool DEBUG_BOTS;

    private float shootRange = 200f;

    private Bot bot;

    private BotMovement botMovement;

    private BotCollisionDetector botCollisionDetector;

    private Pathfinding pathfinding;

    private List<GraphNode> path;

    private int currentIndex;

    private Transform directionIndicator;

    private Graph graph;

    private GameObject goalHolder;

    private int opponentCapturedCounter = 0;

    private bool hasChangedDirection = false;

    public void Start()
    {
        bot = GetComponent<Bot>();

        // register to the events of the bot 
        // if the bot is captured, the bot will be reset after a certain time
        bot.OnCaptured += RestartBotLater;
        // if the bot is uncaptured, the bot will be reset immediately
        bot.OnUncaptured += RestartBotNow;

        botMovement = GetComponent<BotMovement>();

        // register to the event of the bot movement
        // if the bot reaches the goal, the bot will be reset immediately
        botMovement.OnGoalReached += EndInteraction;

        botCollisionDetector = GetComponentInChildren<BotCollisionDetector>();

        // register to the event of the bot collision detector
        botCollisionDetector.OnOpponentBubbbleDetected += AvoidOpponentBubble;

        directionIndicator = transform.Find("Triangle");

        // object holder for the transform of the goal if the interactionId is hort
        goalHolder = new GameObject();
        goalHolder.hideFlags = HideFlags.HideInHierarchy;

        gameManager = FindObjectOfType<GameManager>();
        DEBUG_BOTS = gameManager.GetDebugBots();
    }

    void Update()
    {
        // return if not server
        if (!bot.isServer) return;

        // stop doing stuff on gameOver
        if (gameManager.gameOver) return;

        if (bot.GetChangedInteractionID())
        {
            if (DEBUG_BOTS)
            {
                Debug.Log("Interaction changed to " + bot.GetInteractionID().ToString());
            }
            
            StartInteraction();

            bot.SetChangedInteractionID(false);
        }
    }

    void LateUpdate() 
    {
        if (goal != null)
        {
            LookAtGoal();
        }
    }

    private void RestartBotNow()
    {
        botMovement.StopMoving();
        bot.ResetBot(0f);
    }

    private void RestartBotLater()
    {
        botMovement.StopMoving();
        bot.ResetBot(CharacterBase.BUBBLE_BREAKOUT_TIME);
    }

    /// <summary>
    /// Starts the interaction according to the Interaction ID.
    /// </summary>
    private void StartInteraction()
    {
        int rangeOffset = 0;

        switch (bot.GetInteractionID())
        {
            case InteractionID.Opponent:
                // the opponent should be shot, so the bot needs to stop before the actual goal
                rangeOffset = 5;
                StartCoroutine(CheckIfOpponentMoved());
                break;
            case InteractionID.Teammate:
                break;
            case InteractionID.Diamond:
                break;
            case InteractionID.Hort:
                // to deliver a diamond to the hort, the goal has to be a tile within the hitbox innstead of the center of the hort
                Transform hortGoal = GetFreeTileAroundHort(goal.position);
                SetGoal(hortGoal);
                break;
            case InteractionID.Item:
                break;
            case InteractionID.None:
                RestartBotNow();
                break;
        }

        botMovement.SetTargetPosition(goal.position, rangeOffset);
    }

    private void EndInteraction()
    {
        switch (bot.GetInteractionID())
        {
            case InteractionID.Opponent:
                CharacterBase opponent = goal.parent.GetComponent<CharacterBase>();
                GetComponent<Shooting>().ShootBubble();
                StartCoroutine(CheckIfOpponentCaptured(opponent));
                break;
            case InteractionID.Teammate:
                break;
            case InteractionID.Diamond:
                break;
            case InteractionID.Hort:
                break;
            case InteractionID.Item:
                break;
        }
        RestartBotNow();
    }

    /// <summary>
    /// the bot is following the opponent, check every second if the opponent has moved, 
    /// and if so, restart the bot moving with new position.
    /// </summary>
    private IEnumerator CheckIfOpponentMoved()
    {
        Vector3 lastPosition = goal.position;
        // while the interaction id is still opponent
        while (bot.GetInteractionID() == InteractionID.Opponent)
        {
            Vector3 newPosition = goal.position;
            // if the opponent moved, restart the bot with the new position
            if (newPosition != lastPosition)
            {
                botMovement.SetTargetPosition(newPosition, 5);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Check if the opponent is captured every second for 5 seconds, if they
    /// are captured, increment the opponentCapturedCounter variable by 1 and stop the coroutine.
    /// </summary>
    /// <param name="CharacterBase">the Characterbase of the opponent</param>
    private IEnumerator CheckIfOpponentCaptured(CharacterBase opponent)
    {
        int counter = 0;
        while (counter < 5)
        {
            if (opponent.GetIsCaptured())
            {
                opponentCapturedCounter++;
                yield break;
            }

            counter++;
            yield return new WaitForSeconds(1f);
        }
    }

    private void AvoidOpponentBubble(Transform bubble)
    {
        if (bot.isClient) return;
        RestartBotLater();
        Vector3 direction = bubble.GetComponent<Rigidbody2D>().velocity.normalized;
        Vector3 orthogonal = Vector2.Perpendicular(direction);
        Vector3 avoidPosition = transform.position + orthogonal + Random.onUnitSphere * 1;
        // TODO improve this 
        // TODO maybe add isLeft 
        // Todo Shoot bubble in direction of the opponent

        avoidPosition = GetFreeTileAroundPosition(avoidPosition);

        if(DEBUG_BOTS)
        {
            Debug.DrawLine(transform.position, direction, Color.red, 5f);
            Debug.DrawLine(transform.position, orthogonal, Color.blue, 5f);
            Debug.DrawLine(transform.position, avoidPosition, Color.green, 5f);
        }

        botMovement.SetTargetPosition(avoidPosition);
    }

    /// <summary>
    /// gets a free tile around the given tile in a 3x3 grid
    /// </summary>
    /// <param name="tile">the tile as a Vector2</param>
    /// <returns>a free tile around the given tile</returns>
    public Vector2 GetFreeTileAroundPosition(Vector2 tile)
    {
        Map map = GameObject.Find("Map").GetComponent<Map>();
        Vector2 freeTileAroundTile = tile;

        int x = (int)tile.x;
        int y = (int)tile.y;

        if (map.TileIsFree(x, y))
        {
            return tile;
        }

        int offset = 1;

        while (offset < 10) // TODO: evtl dieses Wert hochsetzen
        {
            int xMin = x - offset;
            int xMax = x + offset;
            int yMin = y - offset;
            int yMax = y + offset;

            List<GraphNode> nodesAroundTile = new List<GraphNode>();
            for (int i = xMin; i <= xMax; i++)
            {
                for (int j = yMin; j <= yMax; j++)
                {
                    nodesAroundTile.Add(graph.GetNode(i, j));
                }
            }

            // order nodes by distance to bot
            nodesAroundTile = nodesAroundTile.OrderBy(node => Vector3.Distance(transform.position, graph.GetWorldPosition(node.GetX(), node.GetY()))).ToList();

            // find closest node to bot that is free
            foreach (GraphNode node in nodesAroundTile)
            {
                if (map.TileIsFree(node.GetX(), node.GetY()))
                {
                    freeTileAroundTile = graph.GetWorldPosition(node.GetX(), node.GetY());
                    return freeTileAroundTile;
                }
            }
            offset++;
        }

        return freeTileAroundTile;
    }

     /// <summary>
    /// gets a free tile around the hort within the hort radius (circle collider)
    /// </summary>
    /// <param name="hortCenter"> the center of the hort</param>
    /// <returns>the transform of the goal</returns>
    private Transform GetFreeTileAroundHort(Vector2 hortCenter)
    {
        Map map = GameObject.Find("Map").GetComponent<Map>();
        Hort hort = bot.GetHort().GetComponent<Hort>();
        int hortScale = map.GetHortScale();

        int radius = (int)hort.gameObject.GetComponent<CircleCollider2D>().radius;

        int x = (int)hortCenter.x;
        int y = (int)hortCenter.y;
        int xMin = x - radius;
        int xMax = x + radius;
        int yMin = y - radius;
        int yMax = y + radius;

        List<GraphNode> nodesAroundHort = new List<GraphNode>();
        for (int i = xMin; i < xMax; i++)
        {
            for (int j = yMin; j < yMax; j++)
            {
                double dx = i - x;
                double dy = j - y;
                double distanceSquared = dx * dx + dy * dy;

                if (distanceSquared <= Mathf.Pow(radius, 2))
                {
                    nodesAroundHort.Add(graph.GetNode(i, j));
                }
            }
        }

        // order nodes by distance to bot
        nodesAroundHort = nodesAroundHort.OrderBy(node => Vector3.Distance(transform.position, graph.GetWorldPosition(node.GetX(), node.GetY()))).ToList();

        // find closest node to bot that is free
        foreach (GraphNode node in nodesAroundHort)
        {
            if (!node.GetIsObstacle())
            {
                goalHolder.transform.position = graph.GetWorldPosition(node.GetX(), node.GetY());
                break;
            }
        }

        return goalHolder.transform;
    }

    /// <summary>
    /// rotates the direction indicator around the bot to look at the goal.
    /// </summary>
    private void LookAtGoal()
    {
        Vector3 lookDir = goal.position - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        directionIndicator.position = transform.position + lookDir.normalized * 2f;
        directionIndicator.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    // determines whether a point is left of a line
    public bool IsLeft(Vector2 a, Vector2 b, Vector2 c)
    {
        //a = line point 1; b = line point 2; c = point
        return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) > 0;
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

    private Vector3 GetPosition()
    {
        return transform.position;
    }

    public void SetGraph(Graph graph)
    {
        this.graph = graph;
    }

    public Graph GetGraph()
    {
        return graph;
    }

    public void SetGoal(Transform goal)
    {
        this.goal = goal;
    }

    public int GetOpponentCapturedCounter()
    {
        return opponentCapturedCounter;
    }
}
