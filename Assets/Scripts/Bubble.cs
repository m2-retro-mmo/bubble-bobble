using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The time in seconds after which the bubble will disappear")]
    private float bubbleLifeTime = 5f;

    private float breakoutTime = 5f;

    private Player capturedPlayer = null;

    private void Start()
    {
        // destroy bubble after 5 seconds if no Player was captured
        Invoke("destroyWhenNothingCaptured", bubbleLifeTime);
    }

    // destroys the bubble when no player is captured
    private void destroyWhenNothingCaptured()
    {
        if (capturedPlayer == null)
        {
            Destroy(gameObject, 0f);
        }
    }

    private void OnDestroy()
    {
        if (capturedPlayer != null)
        {
            capturedPlayer.uncapture();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("hit Player");
            // TODO: check Team of player
            capturedPlayer = collision.gameObject.GetComponent("Player") as Player;
            capturedPlayer.capture();
            // set breakout time
            Destroy(gameObject, breakoutTime);
        }
    }
}
