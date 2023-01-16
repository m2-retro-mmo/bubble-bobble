using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerCollision : MonoBehaviour
{

    private TestCharacterBase player;
    private void Start()
    {
        player = gameObject.GetComponentInParent<TestCharacterBase>();
    }

    /**
    * is called when player | bot collides with another Collider2D
    */
    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (other.gameObject.tag)
        {
            case "Diamond":
                // Collect Diamond if possible
                Diamond diamond = other.GetComponent<Diamond>() as Diamond;
                if (!player.GetHoldsDiamond() && !diamond.GetCollected())
                {
                    diamond.Collect();
                    diamond.SetCollected(true);
                    player.collectDiamond();
                    Debug.Log("character collided with diamond");
                }
                break;

            case "Hort":
                Hort hort = other.gameObject.GetComponent("Hort") as Hort;
                if (hort != null)
                {
                    Debug.Log("character collided with hort");
                    // put diamond into hort
                    if (player.holdsDiamond && player.teamNumber == hort.team)
                    {
                        hort.AddDiamond();
                        player.deliverDiamond();
                    }
                }
                break;
            case "CaptureBubble":
                Debug.Log("character collided with captured player");
                TestCharacterBase capturedPlayer = other.gameObject.GetComponent<TestCaptureBubble>().player;
                // uncapture player if it's a teammate
                if (player.teamNumber == capturedPlayer.GetTeamNumber()){
                    Debug.Log("captured player is a teammate -> uncapture");
                    capturedPlayer.Uncapture();
                }
                break;
            default:
                break;
        }

    }
}
