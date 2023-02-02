using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private CharacterBase player;
    private GameManager gameManager;

    private void Start()
    {
        player = gameObject.GetComponentInParent<CharacterBase>();
        // This script is only needed on the server
        if (player.isClientOnly)
        {
            Destroy(this);
        }
        gameManager = FindObjectOfType<GameManager>();
    }

    /**
    * is called when player | bot collides with another Collider2D
    */
    private void OnTriggerEnter2D(Collider2D other)
    {
        // check if player is set
        if (player == null) return;
        // if gameOver do nothing
        if (gameManager.gameOver) return;
        switch (other.gameObject.tag)
        {
            case "Diamond":
                // Collect Diamond if possible
                Diamond diamond = other.GetComponent<Diamond>() as Diamond;
                if (!player.GetHoldsDiamond() && !diamond.GetCollected())
                {
                    diamond.Collect();
                    player.collectDiamond();
                }
                break;

            case "Hort":
                Hort hort = other.gameObject.GetComponent("Hort") as Hort;
                if (hort != null)
                {
                    // put diamond into hort
                    if (player.holdsDiamond && player.teamNumber == hort.team)
                    {
                        hort.AddDiamond();
                        player.deliverDiamond();
                    }
                }
                break;
            case "CaptureBubble":
                CharacterBase capturedPlayer = other.gameObject.GetComponentInParent<CharacterBase>();
                // uncapture player if it's a teammate
                if (player.teamNumber == capturedPlayer.GetTeamNumber())
                {
                    player.IncrementUncapturedCounter();
                    capturedPlayer.Uncapture();
                }
                break;
            default:
                break;
        }
    }
}
