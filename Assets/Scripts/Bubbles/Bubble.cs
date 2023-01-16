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

    [SerializeField, SyncVar]
    private int teamNumber = -1;

    private bool avoidedByBot = false;
    [SerializeField] private Sprite teamBSprite;

    public override void OnStartServer()
    {
        UpdateAppearance();
        Destroy(gameObject, bubbleLifeTime);
    }
    public override void OnStartClient()
    {
        UpdateAppearance();
    }

    private void UpdateAppearance() {
        if (teamNumber != 1) {
            GetComponent<SpriteRenderer>().sprite = teamBSprite;
        }
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
            collision.gameObject.SendMessage("CaptureCharacter", teamNumber);
        }
        // destroy bubble instantly no matter which collision was detected
        Destroy(gameObject, 0);
    }

    public int GetTeamNumber()
    {
        return this.teamNumber;
    }

    [Server]
    public void SetTeamNumber(int teamNumber)
    {
        this.teamNumber = teamNumber;
    }

    [Server]
    public bool GetAvoidedByBot()
    {
        return avoidedByBot;
    }

    [Server]
    public void SetAvoidedByBot(bool avoided)
    {
        this.avoidedByBot = avoided;
    }
}
