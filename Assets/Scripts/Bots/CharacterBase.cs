using UnityEngine;
using Mirror;

public class CharacterBase : NetworkBehaviour
{
    // States
    [SyncVar] public bool holdsDiamond = false;
    // [SyncVar(hook = nameof(OnIsCapturedChanged))]
    [SyncVar] public bool isCaptured = false;

    // Team
    [SyncVar(hook = nameof(OnTeamNumberChanged))] public byte teamNumber = 1;

    // Movement
    protected Rigidbody2D rb;
    [SerializeField] protected float speed = 5f;

    // Animations
    protected Animator animator;
    [SerializeField] protected Renderer collideableRenderer;
    protected GameObject shape;
    [SerializeField] protected Material teamBMaterial;
    public CaptureBubble captureBubblePrefab;
    private CaptureBubble captureBubble;
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

    // Update is called once per frame
    void Update()
    {

    }

    void SetTeamColor(){
        if (teamNumber != 1) {
            collideableRenderer.material = teamBMaterial;
        }
    }

    void OnTeamNumberChanged(byte oldTeamNumber, byte newTeamNumber)
    {
        SetTeamColor();
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

    /**
    * removes the diamonds from the users inventory
    */
    public void deliverDiamond()
    {
        // TODO: change appearance of dragon here
        holdsDiamond = false;
    }

    public void collectDiamond()
    {
        // TODO: change appearance of dragon here
        holdsDiamond = true;
    }

    /**
    * is triggered when the player got captured by a bubble
    */
    public void Capture()
    {
        SetIsCaptured(true);
        SetPlayerLevel(CAPTURED_LAYER);
        SpawnCaptureBubble();
        Invoke("Uncapture", BUBBLE_BREAKOUT_TIME);
    }

    private void SetPlayerLevel(string levelname){
        // player's shape needs to be set to a level, that does not collide with anything, so that the captureBubble can take over all collisions with others
        shape.layer = LayerMask.NameToLayer(levelname);
    }

    private void SpawnCaptureBubble(){
        // commented out code is not needed if new player movement works

        /*Rigidbody2D rb2 = shape.GetComponent<Rigidbody2D>();
        captureBubble = Instantiate(chaptureBubblePrefab, rb2.position, chaptureBubblePrefab.transform.rotation);*/
        Debug.Log(captureBubblePrefab);
        captureBubble = Instantiate(captureBubblePrefab, transform.position, transform.rotation);
        captureBubble.player = this;
    }

    private void DeleteCaptureBubble(){
        Destroy(captureBubble.gameObject);
    }

    /**
    * uncaptures the dragon 
    */
    public void Uncapture()
    {
        if (isCaptured && captureBubble) {
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
        // Debug.Log("Heelo, Captured changed + " + newIsCaptured);
        SetIsCaptured(newIsCaptured);
        animator.SetBool("isCaptured", newIsCaptured);
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
        animator.SetBool("isCaptured", newIsCaptured);
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
