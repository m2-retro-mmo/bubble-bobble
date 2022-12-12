using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// The class of the Bubble object.
/// </summary>
public class Bubble : NetworkBehaviour
{
    [SerializeField]
    [Tooltip("The time in seconds after which the bubble will disappear")]
    private float bubbleLifeTime = 5f;

    [SerializeField]
    private int teamNumber = -1;

    private void Start()
    {
        // destroy bubble after 5 seconds if no Player was captured
        Destroy(gameObject, bubbleLifeTime);
    }

    /// <summary>
    /// this method is called when the bubble collides with another object
    /// it handles the event that happens 
    /// </summary>
    /// <param name="collision">The collision</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isServer) return;

        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Bot")
        {
            bool foundPlayer = collision.gameObject.TryGetComponent(out Player player); // TODO dieser Code muss schoener werden, rettet die Wale, findet Nemo
            collision.gameObject.TryGetComponent(out Bot bot);
            if (foundPlayer == true)
            {
                player.CaptureCharacter(teamNumber);
            }
            else
            {
                bot.CaptureCharacter(teamNumber);
            }

            // destroy bubble instant
            Destroy(gameObject, 0);
        }
        else if (collision.gameObject.tag == "Bubble")
        {
            Destroy(gameObject, 0.0f);
        }
    }

    public int GetTeamNumber()
    {
        return this.teamNumber;
    }

    public void SetTeamNumber(int teamNumber)
    {
        this.teamNumber = teamNumber;
    }
}
