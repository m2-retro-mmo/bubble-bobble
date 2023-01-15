using System.Collections.Generic;
using UnityEngine;

public class Player : CharacterBase
{
    // Movement
    private Vector2 moveInput;
    private CapsuleCollider2D col;
    private List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    private ContactFilter2D movementFilter;
    public GameObject directionIndicator;
    public GameObject shape;

    public float collisionOffset = 0.1f;

    // direction indicator
    public Vector2 mousePosWorld;
    public Camera cam;
    public Camera mouseCam;
    public Vector3 mousePosScreen = new Vector3();
    public float distanceFactor = 2f;

    public float itemDuration = 0;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        if (isLocalPlayer)
        {
            cam.enabled = true;
            cam.GetComponent<Camera>().enabled = true;
            cam.GetComponent<AudioListener>().enabled = true;

            Cinemachine.CinemachineVirtualCamera cm = GameObject.Find("CineMachine").GetComponent<Cinemachine.CinemachineVirtualCamera>();
            cm.Follow = shape.transform;
            cm.m_Lens.OrthographicSize = 10;
        }
        col = gameObject.GetComponentInChildren<CapsuleCollider2D>();

        LayerMask layermask = LayerMask.GetMask("Player Move Collider");
        movementFilter.SetLayerMask(layermask);
        movementFilter.useLayerMask = true;
    }

    private void LookAtMouse()
    {
        Vector2 playerCenter = rb.position;

        // transform mouse screen coordinates into world coordinates
        mousePosScreen.x = Input.mousePosition.x;
        mousePosScreen.y = Input.mousePosition.y;
        mousePosScreen.z = mouseCam.transform.position.z;
        mousePosWorld = (Vector2)mouseCam.ScreenToWorldPoint(mousePosScreen);

        // rotate the player 
        Vector2 lookDir = mousePosWorld - playerCenter;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        // set position of direction indicator
        directionIndicator.transform.position = (Vector2)playerCenter + lookDir.normalized * distanceFactor;
        directionIndicator.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            GetComponent<Shooting>().CmdShootBubble();
        }
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");

        // shape.transform.position = rb.transform.position;
        Rigidbody2D rb2 = GetComponent<Rigidbody2D>();
        rb2.transform.position = rb.transform.position;
        cam.transform.position = rb.transform.position;
        mouseCam.transform.position = rb.transform.position;
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
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
        if (!isLocalPlayer) return;
        LookAtMouse();
    }

    /**
    * moves the player to an givn direction
    * @direction direction the player should move to
    */
    public bool MovePlayer(Vector2 direction)
    {
        // Cast returns the number of collisions that would accour when moving n the dsired direction
        int count = col.Cast(
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
