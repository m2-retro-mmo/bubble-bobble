using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : CharacterBase
{
    // Movement
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    private ContactFilter2D movementFilter;
    private GameObject directionIndicator;
    public float collisionOffset = 0.1f;
    public static float baseSpeed = 3f;
    public float moveSpeed = 3f;

    // animations
    public Animator animator;

    // direction indicator
    public Vector2 mousePosWorld;
    public Camera cam;
    public Vector3 mousePosScreen = new Vector3();
    public float distanceFactor = 2f;

    public float itemDuration = 0;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = gameObject.GetComponent<BoxCollider2D>();
        directionIndicator = GameObject.FindGameObjectWithTag("directionIndicator");
    }

    private void LookAtMouse()
    {
        Vector2 playerCenter = getCenterOfPlayer();

        // transform mouse screen coordinates into world coordinates
        mousePosScreen.x = Input.mousePosition.x;
        mousePosScreen.y = Input.mousePosition.y;
        mousePosScreen.z = cam.transform.position.z;
        mousePosWorld = (Vector2)cam.ScreenToWorldPoint(mousePosScreen);

        // rotate the player 
        Vector2 lookDir = mousePosWorld - playerCenter;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;


        // set position of direction indicator
        directionIndicator.transform.position = (Vector2)playerCenter + lookDir.normalized * distanceFactor;
        directionIndicator.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    private Vector2 getCenterOfPlayer()
    {
        // player rigidbody is not the center of the player --> use rb.position and add the scaled offset from the collider (=0.825)
        return rb.position + (col.offset * transform.localScale);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            GetComponent<Shooting>().ShootBubble();
        }
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");
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

    private void LateUpdate()
    {
        LookAtMouse();
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

            // set animation trigger
            animator.SetFloat("Horizontal", direction.x);
            animator.SetFloat("Vertical", direction.y);
            animator.SetFloat("Speed", direction.sqrMagnitude);

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
    private void OnTriggerStay2D(Collider2D other)
    {
        // check collision of Player with other Game objects
        switch (other.gameObject.tag)
        {
            case "Hort":
                Hort hort = other.GetComponent("Hort") as Hort;
                if (hort != null)
                {
                    // put diamond into hort
                    if (holdsDiamond && teamNumber == hort.team)
                    {
                        hort.AddDiamond();
                        deliverDiamond();
                    }
                }
                break;
            case "Diamond":
                // collect Diamond if possible
                if (!holdsDiamond)
                {
                    Debug.Log("collision");
                    Debug.Log("HERE IS A DIAMOND");
                    Diamond diamond = other.GetComponent<Diamond>() as Diamond;
                    diamond.collect();
                    collectDiamond();
                }
                else
                {
                    Debug.Log("Player already holds a Diamond!");
                }
                break;
            default:
                break;
        }
    }
}
