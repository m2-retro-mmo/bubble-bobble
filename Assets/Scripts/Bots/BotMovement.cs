using System;
using System.Collections.Generic;
using UnityEngine;

public class BotMovement : MonoBehaviour
{
    public event Action OnGoalReached;

    private Bot bot;

    private Graph graph;

    private Pathfinding pathfinding;

    private List<GraphNode> path;

    private int currentIndex;

    void Start()
    {
        bot = GetComponent<Bot>();
        pathfinding = new Pathfinding(graph);
    }

    void Update()
    {
        if(!bot.GetIsCaptured())
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        if (path != null)
        {
            Vector3 targetPosition = pathfinding.GetGraph().GetWorldPosition((int)path[currentIndex].GetX(), (int)path[currentIndex].GetY());
            if (Vector3.Distance(transform.position, targetPosition) > 1f)
            {
                Vector3 moveDir = (targetPosition - transform.position).normalized;

                float distanceBefore = Vector3.Distance(transform.position, targetPosition);
                bot.SetAnimatorMovement(moveDir);
                transform.position = transform.position + moveDir * bot.GetSpeed() * Time.deltaTime;
            }
            else
            {
                currentIndex++;
                if (currentIndex >= path.Count)
                {
                    StopMoving();
                    bot.SetAnimatorMovement(Vector3.zero);
                    // raise the event that the goal has been reached
                    OnGoalReached?.Invoke();
                }
            }
        }
        else
        {
            bot.SetAnimatorMovement(Vector3.zero); // change back to idle animation
        }
    }

    public void SetTargetPosition(Vector3 targetPosition, int rangeOffset = 0)
    {
        currentIndex = 0;
        path = pathfinding.FindPath(GetPosition(), targetPosition);

        // remove the first node, because it is the current position
        if (path != null && path.Count > 1)
        {
            path.RemoveAt(0);
        }

        // remove last 20 nodes, because they are too close to the target
        if (path != null && path.Count > rangeOffset)
        {
            path.RemoveRange(path.Count - rangeOffset, rangeOffset);
        }
    }

    public void StopMoving()
    {
        path = null;
        graph.ResetGraph();
        pathfinding = new Pathfinding(graph);
    }

    public Vector3 GetPosition()
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
