using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{

    private CharacterBase player;
    private void Start()
    {
        player = gameObject.GetComponentInParent<CharacterBase>();
        // This script is only needed on the server
        if (player.isClientOnly) {
            Destroy(this);
        }
    }

    /**
    * is called when player | bot collides with another Collider2D
    */
    private void OnTriggerStay2D(Collider2D other)
    {
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
            default:
                break;
        }
    }
}
