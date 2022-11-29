using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotShooting : MonoBehaviour
{
    [SerializeField]
    [Tooltip("the point where the bubble will be instantiated")]
    private Transform firePoint;

    [SerializeField]
    private GameObject bubblePrefab;

    [SerializeField]
    [Tooltip("The force with which the bubble is shot")]
    private float bubbleForce = 20f;

    [SerializeField]
    [Tooltip("The time in seconds after which the bubble count will be incremented")]
    private float bubbleCoolDownTime = 3f;

    private int maxBubbleCount = 3;

    private int bubbleCount = 3;

    private float nextIncrementTime = 0;
    
    void Start()
    {
        bubbleCount = maxBubbleCount;
    }

    private void LateUpdate()
    {
        // inrecemt buuble count after every 3 seconds
        if (Time.time >= nextIncrementTime)
        {
            nextIncrementTime = Time.time + bubbleCoolDownTime;
            if (bubbleCount < maxBubbleCount)
            {
                bubbleCount++;
            }
        }
    }

    /// <summary>
    /// Shoots the bubble from the fire point and adds force to it
    /// </summary>
    public void ShootBubble()
    {
        // Check if the bubble count is greater than 0
        if (bubbleCount > 0)
        {
            // spawn bubble and move to direction 
            GameObject bullet = Instantiate(bubblePrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.AddForce(firePoint.up * bubbleForce, ForceMode2D.Impulse);

            if (bubbleCount > 0)
            {
                bubbleCount--;
            }
        }
    }
}
