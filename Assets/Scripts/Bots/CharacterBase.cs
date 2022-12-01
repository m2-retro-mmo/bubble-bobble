using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBase : MonoBehaviour
{
    protected bool holdsDiamond = false;
    protected byte teamNumber = 1;
    protected bool isCaptured = false;

    protected SpriteRenderer spriteRenderer;

    private float bubbleBreakoutTime = 5f;

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
    public void capture()
    {
        // TODO: change appearance to captured player
        isCaptured = true;
        spriteRenderer.color = Color.red;
        Invoke("uncapture", bubbleBreakoutTime);
    }

    /**
    * uncaptures the dragon 
    */
    public void uncapture()
    {
        // TODO: change appearance to uncaptured player
        isCaptured = false;
        spriteRenderer.color = Color.white;
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
