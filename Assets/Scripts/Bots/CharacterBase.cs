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

    // Constants
    public const float BUBBLE_BREAKOUT_TIME = 5f;

    // Start is called before the first frame update
    public virtual void Start()
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

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
        SetIsCaptured(true);
        Invoke("Uncapture", BUBBLE_BREAKOUT_TIME);
    }

    /**
    * uncaptures the dragon 
    */
    public void Uncapture()
    {
        SetIsCaptured(false);
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
