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
    private GameObject directionIndicator;
    public float collisionOffset = 0.1f;
    public static float baseSpeed = 3f;
    public float moveSpeed = 3f;

    // direction indicator
    public Vector2 mousePosWorld;
    public Camera cam;

    // logic
    private bool holdsDiamond = true;
    private byte teamNumber = 0;
    private bool isCaptured = false;

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

    /**
    * removes the diamonds from the users inventory
    */
    private void deliverDiamond()
    {
        // TODO: change appearance of dragon here
        holdsDiamond = false;
    }

    /**
    * is triggered when the player got captured by a bubble
    */
    public void capture()
    {
        // TODO: change appearance to captured player
        isCaptured = true;
    }

    /**
    * uncaptures the dragon 
    */
    public void uncapture()
    {
        // TODO: change appearance to uncaptured player
        isCaptured = false;
    }

    /**
    * set the team number of the player
    */
    public void setTeamNumber(byte newTeamNumber)
    {
        teamNumber = newTeamNumber;
    }

    /**
    * get the players team number
    */
    public byte getTeamNumber()
    {
        return teamNumber;
    }

    /**
    * shoots the bubble in the direction of the mouse cursor
    */
    public void shootBubble()
    {
        // TODO: implement cooldown for shooting bubbles here
        if (bubbleCount > 0)
        {
            // TODO: spawn bubble and move to direction etc.


            bubbleCount -= 1;
        }
        // TODO: if bubble cannot be shooted mabye show that in the HUD?
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        directionIndicator = GameObject.FindGameObjectWithTag("directionIndicator");
    }

    private void LookAtMouse()
    {
        // transform mouse screen coordinates into world coordinates
        Vector3 mousePosScreen = new Vector3();
        mousePosScreen.x = Input.mousePosition.x;
        mousePosScreen.y = Input.mousePosition.y;
        mousePosScreen.z = cam.transform.position.z;
        mousePosWorld = (Vector2)cam.ScreenToWorldPoint(mousePosScreen);

        // rotate the player 
        Vector2 lookDir = mousePosWorld - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        // set position of direction indicator
        directionIndicator.transform.position = rb.position - lookDir.normalized * 3f;
        directionIndicator.transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
    }

    // Update is called once per frame
    void Update()
    {
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");
        LookAtMouse();
    }
    private void FixedUpdate()
    {
        if (!isCaptured)
        {
            bool success = MovePlayer(moveInput);

            // playermovement was not successful check if player can move in only x or only y direction
            if (!success)
            {
                success = MovePlayer(new Vector2(moveInput.x, 0));
                if (!success)
                {
                    success = MovePlayer(new Vector2(0, moveInput.y));
                }
            }
        }
    }

    /**
    * moves the player to an givn direction
    * @direction direction the player should move to
*/
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



    /**
    * is called when player collides with another Collider2D
*/
    private void OnTriggerEnter2D(Collider2D col)
    {
        // check collision of Player with the Hort
        // TODO: instead of compare name we should use tags here!
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
