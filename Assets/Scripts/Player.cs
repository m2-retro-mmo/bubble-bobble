using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    
    private bool holdsDiamond = false;
    private byte teamNumber = null;
    private bool isCaptured = false;
    // TODO: bubble implementation

    // speed 
    public static float baseSpeed = 4; 
    public float speed = 4;

    // bubble settings
    private byte maxBubbleCount = 3;
    private byte bubbleCount = 3;

    public float itemDuration = 0;

    /**
    * checks if the player already holds a diamond, if not the player now holds one
    * @return true successfully collected Diamond, false player already holds a diamond 
    */
    public bool collectDiamond() 
    {
        // TODO: change appearance of dragon to dragon holding diamond
        if (holdsDiamond) return false;
        else holdsDiamond = true;
        return true;
    }

    public bool capture()
    {
        // TODO: change appearance to captured player
        isCaptured = true;
    }

    public void uncapture() 
    {
        isCaptured = false;
    }

    public void setTeamNumber(byte newTeamNumber) 
    {
        teamNumber = newTeamNumber;
    }

    public byte getTeamNumber()
    {
        return teamNumber;
    }

    public void shootBubble()
    {
        // TODO: implement cooldown for shooting bubbles here
        if (bubbleCount > 0) 
        {
            // TODO: spawn bubble and move to direction etc.
        }
        // TODO: if bubble cannot be shooted mabye show that in the HUD?
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(x, y, 0);
        transform.Translate(movement * speed * Time.deltaTime);
    }
}
