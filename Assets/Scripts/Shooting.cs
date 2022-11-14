using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    [SerializeField] 
    private Transform firePoint;
    
    [SerializeField]
    private GameObject bubblePrefab;

    [Header("Bubble Settings")]
    
    [SerializeField]
    [Tooltip("The force with which the bubble is shot")]
    private float bubbleForce = 20f;
    
    [SerializeField]
    [Tooltip("The time in seconds after which the bubble count will be incremented")]
    private float bubbleCoolDownTime = 3f;

    [Header("UI Text")]
    
    [SerializeField]
    private TextMeshProUGUI bubbleCount_text;

    private int maxBubbleCount = 3;
    
    private int bubbleCount = 3;
    
    private float nextIncrementTime = 0;

    void Start()
    {
        bubbleCount = maxBubbleCount;
        ChangeBubbleCount_UI();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            ShootBubble();
        }
    }

    private void LateUpdate()
    {
        // inrecemt buuble count after every 3 seconds
        if (Time.time >= nextIncrementTime)
        {
            nextIncrementTime = Time.time + bubbleCoolDownTime;
            IncrementBubbleCount();
        }
    }

    /// <summary>
    /// Shoots the bubble from the fire point and adds force to it
    /// </summary>
    void ShootBubble()
    {
        // Check if the bubble count is greater than 0
        if (bubbleCount > 0)
        {
            // spawn bubble and move to direction 
            GameObject bullet = Instantiate(bubblePrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.AddForce(firePoint.up * bubbleForce, ForceMode2D.Impulse);

            DecrementBubbleCount();
        }
        else 
        {
            Debug.Log("No more bubbles");
            StartCoroutine(BlinkBubbleCountText());
        } 

    }

    /// <summary>
    /// Decrements the bubble count and checks if the player has no more bubbles left
    /// </summary>
    private void DecrementBubbleCount()
    {
        if(bubbleCount > 0)
        {
            bubbleCount--;
            ChangeBubbleCount_UI();
        }
    }

    /// <summary>
    /// Increments the bubble count if max buuble count is not reached
    /// </summary>
    private void IncrementBubbleCount()
    {
        if(bubbleCount < maxBubbleCount)
        {
            bubbleCount++;
            ChangeBubbleCount_UI();
        }
    }

    /// <summary>
    /// Changes the bubble count text in the UI
    /// </summary>
    private void ChangeBubbleCount_UI()
    {
        bubbleCount_text.fontStyle = FontStyles.Normal;
        bubbleCount_text.text = bubbleCount.ToString();
    }

    /// <summary>
    /// Blinks the bubble count text in the UI for a few seconds
    /// </summary>
    /// <returns>An IEnumerator.</returns>
    private IEnumerator BlinkBubbleCountText()
    {
        bubbleCount_text.fontStyle = FontStyles.Bold;
        yield return new WaitForSeconds(2f);
        bubbleCount_text.fontStyle = FontStyles.Normal;
    }
}
