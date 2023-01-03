using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// This class is responsible for the shooting of the bubbles
/// </summary>
public class Shooting : NetworkBehaviour
{
    [SerializeField]
    [Tooltip("the point where the bubble will be instantiated")]
    private Transform firePoint;

    [SerializeField]
    private GameObject bubblePrefab;

    [Header("Bubble Settings")]

    [SerializeField]
    [Tooltip("The Firerate in seconds after which the next bubble could be shot")]
    private float fireRate = 0.4f;

    [SerializeField]
    [Tooltip("The force with which the bubble is shot")]
    private float bubbleForce = 10f;

    [SerializeField]
    [Tooltip("The time in seconds after which the bubble count will be incremented")]
    private float bubbleCoolDownTime = 3f;

    [Header("UI Text")]

    [SerializeField]
    private TextMeshProUGUI bubbleCount_text;

    private Image bubbleOne;
    private Image bubbleTwo;
    private Image bubbleThree;

    private GameObject character;

    private int maxBubbleCount = 3;

    private int bubbleCount = 3;

    private float nextIncrementTime = 0;

    private float lastShootTime = 0;

    void Start()
    {
        bubbleCount = maxBubbleCount;
        character = this.gameObject;

        if (character.tag == "Player")
        {
            // bubbleCount_text = GameObject.Find("BubbleCountValue_Text").GetComponent<TextMeshProUGUI>();
            bubbleOne = GameObject.Find("FirstBobble").GetComponent<Image>();
            bubbleOne.enabled = true;
            bubbleTwo = GameObject.Find("SecondBobble").GetComponent<Image>();
            bubbleTwo.enabled = true;
            bubbleThree = GameObject.Find("ThirdBobble").GetComponent<Image>();
            bubbleThree.enabled = true;
            ChangeBubbleCount_UI();
        }
    }

    private void Update()
    {
        // inrecemt buuble count after every 3 seconds
        if (!isServer)
        {
            return;
        }
        if (Time.time >= nextIncrementTime)
        {
            nextIncrementTime = Time.time + bubbleCoolDownTime;
            IncrementBubbleCount();
            if (character.tag == "Player")
            {
                ChangeBubbleCount_UI();
            }
        }
    }

    [Command]
    public void CmdShootBubble()
    {
        ShootBubble();
    }

    /// <summary>
    /// Shoots the bubble from the fire point and adds force to it
    /// </summary>
    [Server]
    public void ShootBubble()
    {
        // Check if the bubble count is greater than 0
        if (bubbleCount > 0)
        {
            // check if firerate allows shoting
            if (Time.time > lastShootTime + fireRate)
            {
                // spawn bubble and move to direction 
                GameObject bubble = Instantiate(bubblePrefab, firePoint.position, firePoint.rotation);
                Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
                rb.AddForce(firePoint.up * bubbleForce, ForceMode2D.Impulse);

                int myTeam = character.GetComponent<CharacterBase>().GetTeamNumber();
                bubble.GetComponent<Bubble>().SetTeamNumber(myTeam);

                DecrementBubbleCount();

                NetworkServer.Spawn(bubble);

                if (character.tag == "Player")
                {
                    ChangeBubbleCount_UI();
                    Debug.Log("Player shot bubble");
                }

                lastShootTime = Time.time;
            }
        }
        else
        {
            Debug.Log("No more bubbles");
            /*  Commented this out because i replaced bubble text with bubbles
                if (character.tag == "Player")
                StartCoroutine(BlinkBubbleCountText()); */
        }
    }

    /// <summary>
    /// Decrements the bubble count and checks if the player has no more bubbles left
    /// </summary>
    [Server]
    private void DecrementBubbleCount()
    {
        if (bubbleCount > 0)
        {
            bubbleCount--;
        }
    }

    /// <summary>
    /// Increments the bubble count if max buuble count is not reached
    /// </summary>
    [Server]
    private void IncrementBubbleCount()
    {
        if (bubbleCount < maxBubbleCount)
        {
            bubbleCount++;
        }
    }

    /// <summary>
    /// Changes the bubble count text in the UI
    /// </summary>
    private void ChangeBubbleCount_UI()
    {
        // bubbleCount_text.fontStyle = FontStyles.Normal;
        // bubbleCount_text.text = bubbleCount.ToString();
        // TODO Show bubble count as bubbles
        if (bubbleCount == 1) { 
            bubbleOne.enabled = true;
            bubbleTwo.enabled = false;
            bubbleThree.enabled = false;
        } else if (bubbleCount == 2) {
            bubbleOne.enabled = true;
            bubbleTwo.enabled = true;
            bubbleThree.enabled = false;
        } else if (bubbleCount == 3) {
            bubbleOne.enabled = true;
            bubbleTwo.enabled = true;
            bubbleThree.enabled = true;
        } else {
            bubbleOne.enabled = false;
            bubbleTwo.enabled = false;
            bubbleThree.enabled = false;
            // bubbleCount_text.text = "Keine Bubbles mehr!";
        }
    }

    /// <summary>
    /// Blinks the bubble count text in the UI for a few seconds
    /// </summary>
    /// <returns>An IEnumerator.</returns>
   /*  private IEnumerator BlinkBubbleCountText()
    {
        bubbleCount_text.fontStyle = FontStyles.Bold;
        yield return new WaitForSeconds(2f);
        bubbleCount_text.fontStyle = FontStyles.Normal;
    } */
}
