using UnityEngine;


public class TestCharacterBase : MonoBehaviour
{
    // States
    public bool holdsDiamond = false;
    protected bool isCaptured = false;

    // Team
    [SerializeField] public byte teamNumber = 1;

    // Movement
    protected Rigidbody2D rb;
    [SerializeField] protected float speed = 5f;

    // Animations
    protected Animator animator;
    protected Renderer rend;
    [SerializeField] protected Material teamBMaterial;
    [SerializeField] protected TestCaptureBubble chaptureBubblePrefab;
    private TestCaptureBubble captureBubble;
    public const string CAPTURED_LAYER = "CapturedPlayersLayer";
    private string defaultLayer;

    // Constants
    public const float BUBBLE_BREAKOUT_TIME = 10f;

    // Start is called before the first frame update
    public virtual void Start()
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        rend = transform.Find("Collideable").gameObject.GetComponent<Renderer>();
        SetTeamColor();
        defaultLayer = LayerMask.LayerToName(gameObject.layer);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetTeamColor(){
        if (teamNumber != 1) {
            rend.material = teamBMaterial;
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
}
