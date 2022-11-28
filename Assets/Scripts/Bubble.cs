using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The time in seconds after which the bubble will disappear")]
    private float bubbleLifeTime = 5f;

    [SerializeField]
    private int team = 1;

    private void Start()
    {
        // destroy bubble after 5 seconds if no Player was captured
        Destroy(gameObject, bubbleLifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // TODO: check Team of player
            Player player = collision.gameObject.GetComponent("Player") as Player;
            if (!player.isCaptured)
            {
                if (player.getTeamNumber() != team)
                {
                    player.capture();
                }

                // destroy bubble instant
                Destroy(gameObject, 0);
            }
        }
        else if (collision.gameObject.tag == "Bubble")
        {
            Destroy(gameObject, 0.0f);
        }
    }
}
