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

    private bool avoidedByBot = false;

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

        Debug.Log("Henlo!! - " + collision.gameObject.tag);

        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Bot")
        {
            collision.gameObject.SendMessage("CaptureCharacter", teamNumber);
        }
        // destroy bubble instantly no matter which collision was detected
        Destroy(gameObject, 0);
    }

    public int GetTeamNumber()
    {
        return this.teamNumber;
    }

    public void SetTeamNumber(int teamNumber)
    {
        this.teamNumber = teamNumber;
    }

    public bool GetAvoidedByBot()
    {
        return avoidedByBot;
    }

    public void SetAvoidedByBot(bool avoided)
    {
        this.avoidedByBot = avoided;
    }
}
