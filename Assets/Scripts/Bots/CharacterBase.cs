using UnityEngine;
using Mirror;


public class CharacterBase : NetworkBehaviour
{
    // States
    [SyncVar] protected bool holdsDiamond = false;
    [SyncVar(hook = nameof(OnIsCapturedChanged))] protected bool isCaptured = false;

    // Team
    [SyncVar] [SerializeField] protected byte teamNumber = 1;

    // Movement
    protected Rigidbody2D rb;
    [SerializeField] protected float speed = 5f;

    // Animations
    protected Animator animator;
    [SerializeField] protected Material teamBMaterial;
    [SerializeField] protected CaptureBubble chaptureBubblePrefab;
    private CaptureBubble captureBubble;
    public const string CAPTURED_LAYER = "CapturedPlayersLayer";
    private string defaultLayer;

    // Constants
    public const float BUBBLE_BREAKOUT_TIME = 10f;

    // Start is called before the first frame update
    public virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        SetTeamColor();
        defaultLayer = LayerMask.LayerToName(gameObject.layer);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetTeamColor(){
        if (teamNumber != 1) {
            GetComponent<Renderer>().material = teamBMaterial;
        }
    }

    protected Vector2 transformTargetNodeIntoDirection(Vector3 targetNode)
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

    private void SetAnimatorMovement(Vector2 direction)
    {
        animator.SetFloat("Horizontal", direction.x);
        animator.SetFloat("Vertical", direction.y);
        animator.SetFloat("Speed", direction.sqrMagnitude);
    }

    /**
   * is called when player | bot collides with another Collider2D
   */
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D other)
    {
        // check collision of Player with other Game objects
        switch (other.gameObject.tag)
        {
            case "Hort":
                Hort hort = other.gameObject.GetComponent("Hort") as Hort;
                if (hort != null)
                {
                    Debug.Log("character collided with hort");
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
                Diamond diamond = other.GetComponent<Diamond>() as Diamond;
                if (!GetHoldsDiamond() && !diamond.GetCollected())
                {
                    diamond.collect();
                    collectDiamond();
                    Debug.Log("character collided with diamond");
                }
                break;
            case "CaptureBubble":
                // uncapture player if it's a teammate
                Debug.Log("character collided with captured player");
                TestCharacterBase capturedPlayer = other.gameObject.GetComponent<TestCaptureBubble>().player;
                if (teamNumber == capturedPlayer.GetTeamNumber()){
                    Debug.Log("captured player is a teammate -> uncapture");
                    capturedPlayer.Uncapture();
                }
                break;
            default:
                break;
        }
    }

    /**
    * removes the diamonds from the users inventory
    */
    protected void deliverDiamond()
    {
        // TODO: change appearance of dragon here
        holdsDiamond = false;
    }

    protected void collectDiamond()
    {
        // TODO: change appearance of dragon here
        holdsDiamond = true;
    }

    /**
    * is triggered when the player got captured by a bubble
    */
    public void Capture()
    {
        Debug.Log("Capture");
        SetIsCaptured(true);
        SetPlayerLevel(CAPTURED_LAYER);
        SpawnCaptureBubble();
        Invoke("Uncapture", BUBBLE_BREAKOUT_TIME);
    }

    private void SpawnCaptureBubble(){
        captureBubble = Instantiate(chaptureBubblePrefab, transform.position, transform.rotation);
        captureBubble.player = this;
    }

    private void DeleteCaptureBubble(){
        Destroy(captureBubble.gameObject);
    }

    private void SetPlayerLevel(string levelname){
        // player needs to be set to a level, that does not collide with anything, so that the captureBubble can take over all collisions with others

        gameObject.layer = LayerMask.NameToLayer(levelname);
    }

    /**
    * uncaptures the dragon 
    */
    public void Uncapture()
    {
        if (isCaptured && captureBubble) {
            Debug.Log("uncapture");
            DeleteCaptureBubble();
            SetPlayerLevel(defaultLayer);
            SetIsCaptured(false);
        }
    }

    /**
    * is called when the syncvar isCaptured is changed
    */
    public void OnIsCapturedChanged(bool newIsCaptured, bool oldIsCaptured)
    {
        SetIsCaptured(newIsCaptured);
    }

    public void CaptureCharacter(int teamNumber)
    {
        if (GetTeamNumber() != teamNumber)
        {
            Capture();
        }
    }
    /**
    * set the team number of the player
    */
    public void SetTeamNumber(byte newTeamNumber)
    {
        teamNumber = newTeamNumber;
    }

    /**
    * get the players team number
    */
    public byte GetTeamNumber()
    {
        return teamNumber;
    }

    public void SetIsCaptured(bool newIsCaptured)
    {
        isCaptured = newIsCaptured;
    }

    public bool GetIsCaptured()
    {
        return isCaptured;
    }

    public void SetHoldsDiamond(bool newHoldsDiamond)
    {
        holdsDiamond = newHoldsDiamond;
    }

    public bool GetHoldsDiamond()
    {
        return holdsDiamond;
    }
}
