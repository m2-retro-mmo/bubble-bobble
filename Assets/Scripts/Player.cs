using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Movement
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    private ContactFilter2D movementFilter;

    // movement speed 
    public static float baseSpeed = 3f;
    public float moveSpeed = 3f;
    public float collisionOffset = 0.1f;

    // logic
    private bool holdsDiamond = true;
    private byte teamNumber = 0;
    private bool isCaptured = false;
    // TODO: bubble implementation

    // bubble settings
    private byte maxBubbleCount = 3;
    private byte bubbleCount = 3;
    public float itemDuration = 0;

    /**
    * checks if the player already holds a diamond, if not the player now holds one
    * @return true successfully collected Diamond, false player already holds a diamond 
    */
    public bool collectDiamond()
    {
        // TODO: change appearance of dragon to dragon holding diamond
        if (holdsDiamond) return false;
        else holdsDiamond = true;
        return true;
    }

    private void deliverDiamond()
    {
        // TODO: change appearance of dragon here
        holdsDiamond = false;
    }

    public void capture()
    {
        // TODO: change appearance to captured player
        isCaptured = true;
    }

    public void uncapture()
    {
        // TODO: change appearance to uncaptured player
        isCaptured = false;
    }

    public void setTeamNumber(byte newTeamNumber)
    {
        teamNumber = newTeamNumber;
    }

    public byte getTeamNumber()
    {
        return teamNumber;
    }

    public void shootBubble()
    {
        // TODO: implement cooldown for shooting bubbles here
        if (bubbleCount > 0)
        {
            // TODO: spawn bubble and move to direction etc.
        }
        // TODO: if bubble cannot be shooted mabye show that in the HUD?
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // get movement input
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector2 moveInput = new Vector2(
            moveX,
            moveY
        );

        bool success = MovePlayer(moveInput);

        // playermovement was not successful check if player can move in only x or only y direction
        if (!success)
        {
            success = MovePlayer(new Vector2(moveX, 0));
            if (!success)
            {
                success = MovePlayer(new Vector2(0, moveY));
            }
        }
    }

    public bool MovePlayer(Vector2 direction)
    {
        int count = rb.Cast(
            direction,
            movementFilter,
            castCollisions,
            moveSpeed * Time.fixedDeltaTime + collisionOffset
        );

        if (count == 0)
        {
            Vector2 moveVector = direction * moveSpeed * Time.fixedDeltaTime;

            // no collision
            rb.MovePosition(rb.position + moveVector);
            return true;
        }
        else
        {
            return false;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ApplyDamage(float test)
    {
        Debug.Log(test);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // check collision of Player with the Hort
        // TODO: instea of compare name we should use tags here!
        switch (col.gameObject.name)
        {
            case "Hort":
                // TODO: check if hort belongs to players Team 
                Hort hort = col.GetComponent("Hort") as Hort;
                if (hort != null)
                {
                    // put diamond into hort
                    if (holdsDiamond)
                    {
                        hort.AddDiamond();
                        deliverDiamond();
                    }
                }
                break;
            case "Item":
                // TODO: implement Item collision
                break;

            case "Bubble":
                // TODO: Bubble collision goes here
                break;

            default:
                break;
        }
    }
}
