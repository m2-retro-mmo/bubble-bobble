using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackBotPosition : MonoBehaviour
{
    private Graph graph;

    private Bot bot;

    private GameManager gameManager;

    private bool DEBUG_BOTS;

    // Start is called before the first frame update
    void Start()
    {
        bot = transform.GetComponent<Bot>();
        if (!bot.isServer) {
            Destroy(this);
            return;
        }

        graph = transform.GetComponent<BotMovement>().GetGraph();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent(typeof(GameManager)) as GameManager;
        DEBUG_BOTS = gameManager.GetDebugBots();

        StartCoroutine(TrackPosition());
    }

    private IEnumerator TrackPosition()
    {
        while (true)
        {
            GraphNode oldNode = graph.GetNode(transform.position);
            yield return new WaitForSeconds(5);
            GraphNode newNode = graph.GetNode(transform.position);

            if (oldNode == newNode && !bot.GetIsCaptured())
            {
                if(DEBUG_BOTS)
                    Debug.Log("Bot is on the same position since 3 seconds!");
                bot.ResetBot(0f);
                
            }
        }
    }
}
