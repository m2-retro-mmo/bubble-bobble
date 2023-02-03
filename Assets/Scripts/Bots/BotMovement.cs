using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

/// <summary>
/// This class moves the bot according to the Interaction ID that is set in the BotBehavior class
/// </summary>
public class BotMovement : MonoBehaviour
{
    [HideInInspector]
    public Transform goal;

    private GameManager gameManager;

    private bool DEBUG_BOTS;

    private float shootRange = 200f;

    private Bot bot;

    private Pathfinding pathfinding;

    private List<GraphNode> path;

    private int currentIndex;

    private Transform directionIndicator;

    private Graph graph;

    private GameObject goalHolder;

    private int opponentCapturedCounter = 0;

    public void Start()
    {
        bot = GetComponent<Bot>();
        directionIndicator = transform.Find("Triangle");

        // object holder for the transform of the goal if the interactionId is hort
        goalHolder = new GameObject();
        goalHolder.hideFlags = HideFlags.HideInHierarchy;

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent(typeof(GameManager)) as GameManager;
        DEBUG_BOTS = gameManager.GetDebugBots();
    }

    private void Update()
    {
        // return if not server
        if (!bot.isServer) return;

        if (bot.GetDetectedBubble())
        {
            if (DEBUG_BOTS)
                Debug.Log("Bubble was detected - start avoiding");
            
            bot.ResetBot(3f);

            path = null;
            graph.ResetGraph();
            pathfinding = new Pathfinding(graph);
            CancelInvoke();
            StopAllCoroutines();

            StartCoroutine(AvoidOpponentBubble());

            bot.SetDetectedBubble(false);
        }
        // if the Interaction ID was changed stop everything and start new interaction
        else if (bot.GetChangedInteractionID())
        {
            if (DEBUG_BOTS)
            {
                Debug.Log("Interaction changed to " + bot.GetInteractionID().ToString());
            }

            // reset everythin for new interaction
            path = null;
            graph.ResetGraph();
            pathfinding = new Pathfinding(graph);
            StopAllCoroutines();

            StartInteraction();

            bot.SetChangedInteractionID(false);
        }
        if (goal != null)
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
                if (DEBUG_BOTS)
                    Debug.Log("Start follow opponent");
                StartCoroutine(FollowOpponent());
                break;
            case InteractionID.Teammate:
                if (DEBUG_BOTS)
                    Debug.Log("Start follow teammate");
                StartCoroutine(FollowGoal());
                break;
            case InteractionID.Diamond:
                if (DEBUG_BOTS)
                    Debug.Log("Start follow diamond");
                StartCoroutine(FollowGoal());
                break;
            case InteractionID.Hort:
                if (DEBUG_BOTS)
                    Debug.Log("Start follow hort"); 
                Transform hortGoal = GetFreeTileAroundHort(goal.position);
                SetGoal(hortGoal);
                StartCoroutine(FollowGoal()); 
                break;
            case InteractionID.Item:
                if (DEBUG_BOTS)
                    Debug.Log("Start follow item");
                StartCoroutine(FollowGoal());
                break;
            case InteractionID.None:
                if (DEBUG_BOTS)
                    Debug.Log("Start follow with nothing");
                break;
        }
    }

    IEnumerator FollowGoal()
    {
        InvokeRepeating("CalculatePathToGoal", 1.0f, 0.5f);
        while (goal != null)
        {
            Vector3 botCenter = transform.Find("Collideable").GetComponent<SpriteRenderer>().bounds.center;
            float distToGoal = GetEuclideanDistance(botCenter, goal.position);

            if (path != null)
            {
                Vector3 nextNode = pathfinding.GetGraph().GetWorldPosition((int)path[currentIndex].GetX(), (int)path[currentIndex].GetY());
                float distNextNode = GetEuclideanDistance(botCenter, nextNode);
                Debug.Log("distNextNode: " + distNextNode);
                if (distNextNode <= 20f && currentIndex < path.Count - 1)
                {
                    currentIndex++;
                }

                if (distToGoal <= 0.01f)
                {
                    StopEverything();
                    if (DEBUG_BOTS)
                        Debug.Log("Bot Reached goal");
                    break;
                }
                transform.position = Vector3.MoveTowards(transform.position, nextNode, bot.GetSpeed() * Time.deltaTime);
                

                Vector2 moveDirection = bot.transformTargetNodeIntoDirection(nextNode);
                Debug.Log("Move direction: " + moveDirection);
                if(changedMoveDirection(moveDirection)){
                    bot.SetAnimatorMovement(moveDirection);
                    Debug.Log("Changed move direction");
                }
            }

            yield return new WaitForSeconds(0.001f);
        }
        StopEverything();
    }

    private bool changedMoveDirection(Vector3 moveDirection)
    {
        Vector3 oldMoveDirection = new Vector3(bot.animator.GetFloat("Horizontal"), bot.animator.GetFloat("Vertical"), 0);

        // return false if old x and new x are both greater than 0 or both smaller than 0
        if (oldMoveDirection.x >= 0 && moveDirection.x < 0 || oldMoveDirection.x <= 0 && moveDirection.x > 0)
        {
            return true;
        }

        // return false if old y and new y are both greater than 0 or both smaller than 0
        if (oldMoveDirection.y >= 0 && moveDirection.y < 0 || oldMoveDirection.y <= 0 && moveDirection.y > 0)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Follows the opponent.
    /// if opponent is in shooting range, shoot opponent.
    /// </summary>
    /// <returns>An IEnumerator.</returns>
    IEnumerator FollowOpponent()
    {
        InvokeRepeating("CalculatePathToGoal", 1.0f, 0.5f);

        CharacterBase opponent = goal.parent.GetComponent<CharacterBase>(); 

        while (true)
        {
            if (goal == null)
                break;

            Vector3 botCenter = transform.Find("Collideable").GetComponent<SpriteRenderer>().bounds.center;
            float distToPlayer = GetEuclideanDistance(botCenter, goal.position);

            if (path != null)
            {
                Vector3 nextNode = pathfinding.GetGraph().GetWorldPosition((int)path[currentIndex].GetX(), (int)path[currentIndex].GetY());
                float distNextNode = GetEuclideanDistance(botCenter, nextNode);
                Debug.Log("distNextNode: " + distNextNode);
                if (distNextNode <= 20f && currentIndex < path.Count - 1)
                {
                    currentIndex++;
                }

                if (distToPlayer <= shootRange) // TODO: check if player is captured, if so find new goal
                {
                    GetComponent<Shooting>().ShootBubble();
                    StopEverything();
                    StartCoroutine(CheckIfOpponentCaptured(opponent));
                    break;
                }
                transform.position = Vector3.MoveTowards(transform.position, nextNode, bot.GetSpeed() * Time.deltaTime);

                Vector2 moveDirection = bot.transformTargetNodeIntoDirection(nextNode);
                Debug.Log("Move direction: " + moveDirection);
                if(changedMoveDirection(moveDirection)){
                    bot.SetAnimatorMovement(moveDirection);
                    Debug.Log("Changed move direction");
                }
            }
            else if (distToPlayer >= (shootRange + 5f))
            {
                InvokeRepeating("CalculatePathToGoal", 0.1f, 0.5f);
            }

            if (opponent.GetIsCaptured())
            {
                if (DEBUG_BOTS)
                    Debug.Log("Opponent captured");
                StopEverything();
                break;
            }

            yield return new WaitForSeconds(0.001f);
        }
    }

    IEnumerator CheckIfOpponentCaptured(CharacterBase opponent)
    {
        int counter = 0;
        while(counter < 5)
        {
            if (opponent.GetIsCaptured())
            {
                opponentCapturedCounter++;
                yield break;
            }

            yield return new WaitForSeconds(1f);
            counter++;
        }
    }

    IEnumerator AvoidOpponentBubble()
    {
        if (DEBUG_BOTS)
            Debug.Log("Avoid opponent bubble");

        if (goal == null)
        {
            if (DEBUG_BOTS)
                Debug.Log("Goal is null");
            yield break;
        }

        Vector3 botCenter = transform.Find("Collideable").GetComponent<SpriteRenderer>().bounds.center;
        float distToBubble = GetEuclideanDistance(botCenter, goal.position);

        // if the bubble is closer than shootRange move away from it 
        if (distToBubble < (shootRange + 200f))// TODO: evtl hier den Bereich kleiner machen
        {
            if (DEBUG_BOTS)
                Debug.Log("Bubble is closer than shoot range");

            Vector3 oldBubblePos = goal.position;
            yield return new WaitForSeconds(0.01f);
            if (goal == null)
            {
                if (DEBUG_BOTS)
                    Debug.Log("Goal is null");
                StopEverything();
                yield break;
            } 
            Vector3 newBubblePos = goal.position;

            Vector3 avoidPosition = CalculateAvoidPosition(oldBubblePos, newBubblePos);

            if (DEBUG_BOTS)
                Debug.Log("Goal to avoid bubble: " + avoidPosition.ToString());

            Debug.DrawLine(transform.position, avoidPosition, Color.magenta, 5f);

            CalculatePathToGoal(avoidPosition);


            while (true)
            {
                if (goal == null)
                {
                    if (DEBUG_BOTS)
                        Debug.Log("Goal is null");
                    StopEverything();
                    yield break;
                }

                float distToGoal = GetEuclideanDistance(botCenter, avoidPosition);

                if (path != null)
                {
                    Vector3 nextNode = pathfinding.GetGraph().GetWorldPosition((int)path[currentIndex].GetX(), (int)path[currentIndex].GetY());
                    float distNextNode = GetEuclideanDistance(botCenter, nextNode);
                    Debug.Log("distNextNode: " + distNextNode);
                    if (distNextNode <= 20f && currentIndex < path.Count - 1)
                    {
                        currentIndex++;
                    }

                    if (distToGoal <= 0.25f)
                    {
                        if (DEBUG_BOTS)
                            Debug.Log("Bot avoided Bubble"); 
                        StopEverything();
                        break;
                    }
                    transform.position = Vector3.MoveTowards(transform.position, nextNode, bot.GetSpeed() * Time.deltaTime);

                    Vector2 moveDirection = bot.transformTargetNodeIntoDirection(nextNode);
                    Debug.Log("Move direction: " + moveDirection);
                    if(changedMoveDirection(moveDirection)){
                        bot.SetAnimatorMovement(moveDirection);
                        Debug.Log("Changed move direction");
                    }
                }

                yield return new WaitForSeconds(0.001f);
            }
        }
        // if the bubble is further away than shootRange shoot it 
        else
        {
            if (DEBUG_BOTS)
                Debug.Log("Bubble is further away than shoot range");
            GetComponent<Shooting>().ShootBubble(); // TODO bubble schießt manchmal in falsche richtung

            if (DEBUG_BOTS)
                Debug.Log("Shot Bubble");
            StopEverything();
        }
        }

    private Vector3 CalculateAvoidPosition(Vector2 oldBubblePos, Vector2 newBubblePos)
    {
        Vector2 botPos = new Vector2(transform.position.x, transform.position.y);
        // VECTOR
        Vector2 optimalShoot = oldBubblePos - botPos;
        float lengthOptimalShoot = optimalShoot.magnitude;

        // VECTOR calculate where the bubble will be with the length of the optimal shoot
        Vector2 shootDir = newBubblePos - oldBubblePos;
        // make sure the shootDir has the same length as the optimalShoot
        shootDir = shootDir.normalized * lengthOptimalShoot;

        // POINT the point on shootDir at height of the bot
        Vector2 shootGoal = shootDir + oldBubblePos;

        // avoid bubble if distance between bot and calculated shoot path is smaller than x
        if (GetEuclideanDistance((Vector3)shootGoal, (Vector3)botPos) <= 5f)// TODO: check if float distance = (optimalShoot + shootDir).magnitude is better
        {
            Debug.DrawLine(oldBubblePos, shootGoal, Color.green, 5f);

            // VECTOR 
            Vector2 orthogonal = Vector2.Perpendicular(shootDir);

            Vector2 orthoGoal = orthogonal + oldBubblePos;

            // check if bot is on the same side of the shootDir vector as the orthoGoal
            bool sameSide = (IsLeft(oldBubblePos, shootGoal, botPos) == IsLeft(oldBubblePos, shootGoal, orthoGoal));

            if (!sameSide)
            {
                if (DEBUG_BOTS)
                    Debug.Log("bot is on the right of shootDir");
                orthogonal = -orthogonal;
            }

            Debug.DrawLine(oldBubblePos, orthoGoal, Color.blue, 5f);

            Vector2 botGoal = orthogonal + shootGoal;

            Debug.DrawLine(botPos, botGoal, Color.red, 5f);

            // TODO check if goal is free tile
            botGoal = GetFreeTileAroundPosition(botGoal);

            // TODO randomness draufrechnen damit bot nicht immer in gleiche richtung ausweicht

            return botGoal;
        }
        return transform.position;
    }

    // determines whether a point is left of a line
    public bool IsLeft(Vector2 a, Vector2 b, Vector2 c)
    {
        //a = line point 1; b = line point 2; c = point
        return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) > 0;
    }

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
    /// Calculates the path to goal and resets the path index.
    /// gets invoked
    /// </summary>
    private void CalculatePathToGoal()
    {
        if(goal == null)
            return;
        path = pathfinding.FindPath(transform.position, goal.position);
        currentIndex = 0;
    }

    private void CalculatePathToGoal(Vector3 goalPos)
    {
        path = pathfinding.FindPath(transform.position, goalPos);
        currentIndex = 0;
    }

    public void StopEverything()
    {
        bot.SetAnimatorMovement(Vector2.zero);
        CancelInvoke();
        StopAllCoroutines();
        bot.ResetBot(0f);
        path = null;
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

        directionIndicator.position = transform.position + lookDir.normalized * 2f;
        directionIndicator.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

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
