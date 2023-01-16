using UnityEngine;
using Mirror;

public class CharacterBase : NetworkBehaviour
{
    // States
    [SyncVar] public bool holdsDiamond = false;
    [SyncVar(hook = nameof(OnCaptureChanged))] public bool isCaptured = false;

    // Team
    [SyncVar] public byte teamNumber = 1;

    // Movement
    protected Rigidbody2D rb;
    [SerializeField] protected float speed = 5f;

    // Animations
    protected Animator animator;
    [SerializeField] protected Renderer collideableRenderer;
    protected GameObject shape;
    [SerializeField] protected Material teamBMaterial;
    [SerializeField] protected Sprite captureBobbleBSprite;
    [SerializeField] private GameObject captureBubble;
    public const string CAPTURED_LAYER = "CapturedPlayersLayer";
    private string defaultLayer;
    // Constants
    public const float BUBBLE_BREAKOUT_TIME = 10f;

    // Start is called before the first frame update
    public virtual void Start()
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        shape = transform.Find("Shape").gameObject;
        defaultLayer = LayerMask.LayerToName(shape.layer);
        SetTeamColor();
    }

    public override void OnStartClient()
    {
        SetTeamColor();
    }

    void SetTeamColor()
    {
        if (teamNumber != 1)
        {
            collideableRenderer.material = teamBMaterial;
            captureBubble.GetComponent<SpriteRenderer>().sprite = captureBobbleBSprite;
        }
    }

    public Vector2 transformTargetNodeIntoDirection(Vector3 targetNode)
    {
        Vector2 target = targetNode;
        Vector2 moveDirection = (target - rb.position);
        moveDirection.Normalize();  // or maybe Normalize(moveDirection);
        return moveDirection;
    }

    protected void Move(Vector2 direction)
    {
        Vector2 moveVector = direction * speed * Time.fixedDeltaTime;
        SetAnimatorMovement(direction);
        rb.MovePosition(rb.position + moveVector);
    }

    public void SetAnimatorMovement(Vector2 direction)
    {
        animator.SetFloat("Horizontal", direction.x);
        animator.SetFloat("Vertical", direction.y);
        animator.SetFloat("Speed", direction.sqrMagnitude);
    }

    [Server]
    public void deliverDiamond()
    {
        // TODO: change appearance of dragon here
        holdsDiamond = false;
    }
    [Server]
    public void collectDiamond()
    {
        // TODO: change appearance of dragon here
        holdsDiamond = true;
    }

    /**
    * is triggered when the player got captured by a bubble
    */
    [Server]
    public void Capture(int fromTeamNumber)
    {
        if (isCaptured) return;
        if (teamNumber == fromTeamNumber) return;
        isCaptured = true;
        Invoke("Uncapture", BUBBLE_BREAKOUT_TIME);
        CaptureStateUpdate();
    }

    /**
    * uncaptures the dragon 
    */
    [Server]
    public void Uncapture()
    {
        if (!isCaptured) return;
        isCaptured = false;
        CaptureStateUpdate();
    }

    [Client]
    private void OnCaptureChanged(bool oldIsCaptured, bool newIsCaptured)
    {
        CaptureStateUpdate();
    }

    // [Both]
    private void CaptureStateUpdate()
    {
        animator.SetBool("isCaptured", isCaptured);
        captureBubble.SetActive(isCaptured);
        SetPlayerLevel(isCaptured ? CAPTURED_LAYER : defaultLayer);
    }

    private void SetPlayerLevel(string levelname)
    {
        // player's shape needs to be set to a level, that does not collide with anything,
        // so that the captureBubble can take over all collisions with others
        shape.layer = LayerMask.NameToLayer(levelname);
    }

    /**
    * set the team number of the player
    */
    [Server]
    public void SetTeamNumber(byte newTeamNumber)
    {
        teamNumber = newTeamNumber;
    }

    /**
    * [Both] get the players team number
    */
    public byte GetTeamNumber()
    {
        return teamNumber;
    }

    // [Both]
    public bool GetIsCaptured()
    {
        return isCaptured;
    }

    [Server]
    public void SetHoldsDiamond(bool newHoldsDiamond)
    {
        holdsDiamond = newHoldsDiamond;
    }

    public bool GetHoldsDiamond()
    {
        return holdsDiamond;
    }

    [Server]
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public float GetSpeed()
    {
        return speed;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = teamNumber == 1 ? Color.red : Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(3, 3, 0));
    }
}
