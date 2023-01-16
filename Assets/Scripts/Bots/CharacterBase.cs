using UnityEngine;
using Mirror;


public class CharacterBase : NetworkBehaviour
{
    // States
    [SyncVar] public bool holdsDiamond = false;
    [SyncVar(hook = nameof(OnIsCapturedChanged))] protected bool isCaptured = false;

    // Team
    [SyncVar][SerializeField] public byte teamNumber = 1;

    // Movement
    protected Rigidbody2D rb;
    [SerializeField] protected float speed = 5f;

    // Animations
    protected Animator animator;
    protected Renderer rend;
    protected GameObject shape;
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
        rb = GetComponentInChildren<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        rend = transform.Find("Collideable").gameObject.GetComponent<Renderer>();
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
        //Vector2 moveVector = direction * speed * Time.fixedDeltaTime;
        //rb.MovePosition(rb.position + moveVector);

        SetAnimatorMovement(direction);

        // new movement via tranform.position to fix bug, that only the rigidbody of the character'parent node is moved, but not the transform or it's children
        transform.position = transform.position + new Vector3(direction.x * speed * Time.deltaTime, direction.y * speed * Time.deltaTime, 0);
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
        // commented out code is not needed if new player movement works

        /*Rigidbody2D rb2 = shape.GetComponent<Rigidbody2D>();
        captureBubble = Instantiate(chaptureBubblePrefab, rb2.position, chaptureBubblePrefab.transform.rotation);*/
        captureBubble = Instantiate(chaptureBubblePrefab, transform.position, transform.rotation);
        captureBubble.player = this;
    }

    private void DeleteCaptureBubble(){
        Destroy(captureBubble.gameObject);
    }

    private void SetPlayerLevel(string levelname){
        // player's shape needs to be set to a level, that does not collide with anything, so that the captureBubble can take over all collisions with others
        shape.layer = LayerMask.NameToLayer(levelname);
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
