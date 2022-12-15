using UnityEngine;
using Mirror;


public class CharacterBase : NetworkBehaviour
{
    [SyncVar] protected bool holdsDiamond = false;
    [SyncVar] protected byte teamNumber = 1;
    [SyncVar(hook = nameof(OnIsCapturedChanged))] protected bool isCaptured = false;

    protected SpriteRenderer spriteRenderer;

    public const float BUBBLE_BREAKOUT_TIME = 5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /**
   * is called when player | bot collides with another Collider2D
   */
    [ServerCallback]
    private void OnTriggerStay2D(Collider2D other)
    {
        // check collision of Player with other Game objects
        switch (other.gameObject.tag)
        {
            case "Hort":
                Hort hort = other.GetComponent("Hort") as Hort;
                if (hort != null)
                {
                    Debug.Log("bot collided with hort");
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
                    Debug.Log("bot collided with diamond");
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
        // TODO: change appearance to captured player
        isCaptured = true;
        spriteRenderer.color = Color.red;
        Invoke("Uncapture", BUBBLE_BREAKOUT_TIME);
    }

    /**
    * uncaptures the dragon 
    */
    public void Uncapture()
    {
        // TODO: change appearance to uncaptured player
        isCaptured = false;
        spriteRenderer.color = Color.white;
    }

    /**
    * is called when the syncvar isCaptured is changed
    */
    public void OnIsCapturedChanged(bool newIsCaptured, bool oldIsCaptured)
    {
        if (newIsCaptured)
        {
            spriteRenderer.color = Color.white;
        }
        else
        {
            spriteRenderer.color = Color.red;
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
