using UnityEngine;
using Mirror;


public class CharacterBase : NetworkBehaviour
{
    [SyncVar] protected bool holdsDiamond = false;
    [SyncVar] protected byte teamNumber = 1;
    [SyncVar] protected bool isCaptured = false;

    protected SpriteRenderer spriteRenderer;

    protected const float BUBBLE_BREAKOUT_TIME = 5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
