using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The class of the Bubble object.
/// </summary>
public class Bubble : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The time in seconds after which the bubble will disappear")]
    private float bubbleLifeTime = 5f;

    private void Start()
    {
        // destroy bubble after 5 seconds
        Destroy(gameObject, bubbleLifeTime);
    }

    /// <summary>
    /// this method is called when the bubble collides with another object
    /// it handles the event that happens 
    /// </summary>
    /// <param name="collision">The collision</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("hit Player");
        }
    }
}
