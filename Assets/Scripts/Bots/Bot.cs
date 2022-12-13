using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using UnityEditor.Compilation;

/// <summary>
/// the states of the InteractionID
/// </summary>
public enum InteractionID
{
    Opponent = 0,
    Teammate = 1,
    OpponentBubble = 2,
    Diamond = 3,
    Hort = 4,
    Item = 5,
    None = 6
}

/// <summary>
/// This class handles the bot behavior
/// it checks the area around the bot and sets the Interaction id according to the colliders in the area
/// the interactionId defines what the bot should do
/// </summary>
public class Bot : CharacterBase
{
    public InteractionID interactionID = InteractionID.None;

    [HideInInspector]
    public bool foundInteraction;

    [HideInInspector]
    public bool changedInteractionID = false;

    // the weights of the interactions
    private float[] interactionWeights = new float[] { 2, 5, 6, 4, 3, 1 };

    private float[] interactionPriorities;

    private Transform[] interactionGoals;

    // area radius around the bot
    private float interactionRadius = 10f;

    private Vector3 botPosition;

    private Collider2D[] colliders;

    private BotMovement botMovement;

    private int prevPriorityIndex = -1;

    public void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        botMovement = GetComponent<BotMovement>();
    }
    [Server]
    void Start()
    {
        teamNumber = 0; // TODO: sp�ter anders l�sen, nur zum testen
    }

    public void ResetBot()
    {
        prevPriorityIndex = -1;
        StopAllCoroutines();
        SetInteractionID(InteractionID.None);
        SetChangedInteractionID(false);

        Invoke("StartBot", BUBBLE_BREAKOUT_TIME);
    }

    public void StartBot()
    {
        StartCoroutine(CheckAreaOfInterest());
    }

    /// <summary>
    /// Checks the area around the bot with a given radius 
    /// it sets the Interaction ID to the interaction with the highest priority
    /// </summary>
    /// <returns>An IEnumerator.</returns>
    IEnumerator CheckAreaOfInterest()
    {
        yield return new WaitForSeconds(1f);
        // check area every ten seconds
        while (true)
        {
            Debug.Log("start searching for interaction...");
            foundInteraction = false;
            // reset all priority values
            interactionPriorities = new float[6];
            interactionGoals = new Transform[6];

            botPosition = transform.position;
            // get all colliders in a radius around the bot
            colliders = Physics2D.OverlapCircleAll(botPosition, interactionRadius);

            CheckForOpponents();
            CheckForTeammates();

            if(GetHoldsDiamond())
            {
                CheckForDiamond();
            } else
            {
                interactionPriorities[(int)InteractionID.Hort] += 1f;
                CheckForHort();
            }

            CalculateInteractionID();

            //Debug.Log("found Interaction " + foundInteraction);
            //Debug.Log("changedInteraction " + changedInteractionID);
            //Debug.Log("interactionID: " + interactionID);

            yield return new WaitForSeconds(10f);
        }
    }

    /// <summary>
    /// Checks the for opponents in the area around the bot and sets the interaction priorities
    /// </summary>
    private void CheckForOpponents()
    {
        // get all opponents in the area
        Collider2D[] opponentColliders = GetCollidersByTeamNumber(GetOpponentTeamNumber(teamNumber));

        // get all opponents who are holding a diamond
        Collider2D[] opponentWithDiamondColliders = opponentColliders.Where(c => c.GetComponent<CharacterBase>().GetHoldsDiamond() == true).ToArray();
        
        float opponentPriority = 1f;

        // if there are opponents who are holding a diamond set the opponents array to the opponents who are holding a diamond
        if (opponentWithDiamondColliders.Length > 0)
        {
            opponentColliders = opponentWithDiamondColliders;
            // priority for opponents who are holding a diamond is higher
            opponentPriority = 2f;
        }

        // if there are no opponents do nothing
        if (opponentColliders.Length > 0)
        {
            // loop through all opponents 
            foreach (Collider2D collider in opponentColliders)
            {
                CharacterBase opponent = collider.gameObject.GetComponent<CharacterBase>();

                // check if the opponent is not captured
                if (!opponent.GetIsCaptured()) // TODO
                {
                    // set the priority for the opponent interaction
                    interactionPriorities[(int)InteractionID.Opponent] += opponentPriority;

                    // multiply interactionPriority with interactionWeight
                    interactionPriorities[(int)InteractionID.Opponent] *= interactionWeights[(int)InteractionID.Opponent];

                    // set the interactionGoal to the opponent
                    interactionGoals[(int)InteractionID.Opponent] = opponent.transform;

                    foundInteraction = true;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Checks the for teammates in the area around the bot and sets the interaction priorities
    /// </summary>
    private void CheckForTeammates()
    {
        // get all teammates in the area
        Collider2D[] teammateColliders = GetCollidersByTeamNumber(teamNumber);

        // if there are no teammates do nothing
        if(teammateColliders.Length > 0)
        {
            // loop through all teammates
            foreach (Collider2D collider in teammateColliders)
            {
                CharacterBase teammate = collider.gameObject.GetComponent<CharacterBase>();

                // check if the teammate is captured - if he is, free teammate
                if (teammate.GetIsCaptured())
                {
                    interactionPriorities[(int)InteractionID.Teammate] += 1f;

                    // multiply interactionPriority with interactionWeight
                    interactionPriorities[(int)InteractionID.Teammate] *= interactionWeights[(int)InteractionID.Teammate];

                    // set the interactionGoal to teammate
                    interactionGoals[(int)InteractionID.Teammate] = teammate.transform;

                    foundInteraction = true;
                    break;
                }
            }
        }
    }

    private void CheckForOpponentBubbles()
    {
        // get all opponent bubbles in the area 
        Collider2D[] opponentBubbleColliders = GetCollidersByTag("Bubble");
    }

    /// <summary>
    /// Checks for diamonds in the area around the bot if the bot does not hold a diamond and sets the interaction priorities accordingly
    /// </summary>
    private void CheckForDiamond()
    {
        // get all diamonds in the area
        Collider2D[] diamondColliders = GetCollidersByTag("Diamond");

        // are there diamonds near by
        if (diamondColliders.Length > 0)
        {
            // loop through all opponents 
            foreach (Collider2D collider in diamondColliders)
            {
                Diamond diamond = collider.gameObject.GetComponent<Diamond>();
                // has a higher priority to collect a diamond
                interactionPriorities[(int)InteractionID.Diamond] += 1f;

                // multiply interactionPriority with interactionWeight
                interactionPriorities[(int)InteractionID.Diamond] *= interactionWeights[(int)InteractionID.Diamond];

                // set the interactionGoal to diamond
                interactionGoals[(int)InteractionID.Diamond] = diamond.transform;

                foundInteraction = true;
            }
        }
    }

    /// <summary>
    /// Checks for bot's hort if the bot holds a diamond and ets the interaction priorities
    /// </summary>
    private void CheckForHort()
    {
        if(!GetHoldsDiamond())
        {
            return;
        }
        // get all diamonds in the area
        Collider2D[] hortColliders = GetCollidersByTag("Collider");

        // are there diamonds near by
        if (hortColliders.Length > 0)
        {
            // loop through all opponents 
            foreach (Collider2D collider in hortColliders)
            {
                Hort hort = collider.gameObject.GetComponent<Hort>();
                // has a higher priority to drop diamond
                interactionPriorities[(int)InteractionID.Hort] += 1f;

                // multiply interactionPriority with interactionWeight
                interactionPriorities[(int)InteractionID.Hort] *= interactionWeights[(int)InteractionID.Hort];

                // set the interactionGoal to hort
                interactionGoals[(int)InteractionID.Hort] = hort.transform;

                foundInteraction = true;
            }
        }
    }
    /// <summary>
    /// Gets the colliders around the bot by tag.
    /// orders by distance from the bot 
    /// </summary>
    /// <param name="tagName">The tag name.</param>
    /// <returns>An array of Collider2DS.</returns>
    private Collider2D[] GetCollidersByTag(string tagName)
    {
        colliders = colliders.Where(c => c.gameObject.tag == tagName).ToArray();
        // order by distance
        colliders = colliders.OrderBy(c => Vector3.Distance(botPosition, c.transform.position)).ToArray();

        return colliders;
    }

    /// <summary>
    /// Gets the colliders by team number.
    /// either bots or player
    /// </summary>
    /// <param name="teamNumber">The team number.</param>
    /// <returns>An array of Collider2DS.</returns>
    private Collider2D[] GetCollidersByTeamNumber(int teamNumber)
    {
        colliders = colliders.Where(c => 
            (c.gameObject.TryGetComponent(out Player player) && player.GetTeamNumber() == teamNumber) ||
            (c.gameObject.TryGetComponent(out Bot bot) && bot.GetTeamNumber() == teamNumber)).ToArray();
        // order by distance
        colliders = colliders.OrderBy(c => Vector3.Distance(botPosition, c.transform.position)).ToArray();

        return colliders;
    }

    /// <summary>
    /// sets the interactionID to the highest priority value
    /// if no interaction was found, increase the radius of the area
    /// </summary>
    private void CalculateInteractionID()
    {
        // if an interaction was found, set the interactionID to highest priority
        if (foundInteraction)
        {
            // get index of highest value in priority array
            int highestPriorityIndex = interactionPriorities.ToList().IndexOf(interactionPriorities.Max());
            // cast this index to an InteractionID
            InteractionID foundInteractionID = (InteractionID)highestPriorityIndex;

            // check: did we find a new interactionId that is higher prioritized the old interaction
            if (foundInteractionID != interactionID && highestPriorityIndex > prevPriorityIndex) // TODO: only change id if according priority is a given amount higher than priority of old id
            {
                Debug.Log("set goal to: " + foundInteractionID);
                prevPriorityIndex = highestPriorityIndex;
                changedInteractionID = true;
                interactionID = foundInteractionID;
                // set goal of bot movement to goal position
                botMovement.goal = interactionGoals[highestPriorityIndex];
            }
        }
        else // if no interaction was found, increase the radius
        {
            interactionRadius += 10f;
        }
    }

    // TODO: in gameManager verschieben?
    /// <summary>
    /// Gets the opponent team number.
    /// </summary>
    /// <param name="myTeamNumber">The my team number.</param>
    /// <returns>A byte.</returns>
    private byte GetOpponentTeamNumber(byte myTeamNumber)
    {
        return (byte)(myTeamNumber == 0 ? 1 : 0);
    }

    public bool GetChangedInteractionID()
    {
        return changedInteractionID;
    }

    public void SetChangedInteractionID(bool value)
    {
        changedInteractionID = value;
    }

    public InteractionID GetInteractionID()
    {
        return interactionID;
    }

    public void SetInteractionID(InteractionID newInteractionID)
    {
        interactionID = newInteractionID;
    }
}
