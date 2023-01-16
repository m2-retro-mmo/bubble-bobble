using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TestPlayer : TestCharacterBase
{
    // Movement
    private Vector2 moveInput;
    private BoxCollider2D col;
    private List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    private ContactFilter2D movementFilter;
    public GameObject directionIndicator;
    public GameObject collidable;

    public float collisionOffset = 0.1f;

    // direction indicator
    public Vector2 mousePosWorld;
    public Camera cam;
    public Vector3 mousePosScreen = new Vector3();
    public float distanceFactor = 2f;

    public float itemDuration = 0;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        cam.enabled = true;
        cam.GetComponent<Camera>().enabled = true;
        cam.GetComponent<AudioListener>().enabled = true;

        col = gameObject.GetComponentInChildren<BoxCollider2D>();

        LayerMask layermask = LayerMask.GetMask("Player Move Collider");
        movementFilter.SetLayerMask(layermask);
        movementFilter.useLayerMask = true;
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
        directionIndicator.transform.position = (Vector2)playerCenter - lookDir.normalized * distanceFactor;
        directionIndicator.transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
    }

    private Vector2 getCenterOfPlayer()
    {
        // player rigidbody is not the center of the player --> use rb.position and add the scaled offset from the collider (=0.825)
        return rb.position + (col.offset * shape.transform.localScale);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            GetComponent<TestShooting>().CmdShootBubble();
        }
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");

        // not needed anymore, if new Movement via transform.position works properly
        //Rigidbody2D rb2 = shape.GetComponent<Rigidbody2D>();
        //rb2.transform.position = rb.transform.position;
        //cam.transform.position = rb.transform.position;
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
        // Cast returns the number of collisions that would accour when moving n the dsired direction
        int count = rb.Cast(
            direction,
            movementFilter,
            castCollisions,
            speed * Time.fixedDeltaTime + collisionOffset
        );

        if (count > 0)
        {
            // Player can't move in the disired direction without collisions
            return false;
        }

        Move(direction);
        return true;
    }
}
