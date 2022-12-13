using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

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
        StopAllCoroutines();
        SetInteractionID(InteractionID.None);
        SetChangedInteractionID(false);

        Invoke("StartBot", BUBBLE_BREAKOUT_TIME);
    }

    public void StartBot()
    {
        Debug.Log("Start Bot");
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
                    // has a higher priority if opponent holds diamond
                    if (opponent.GetHoldsDiamond())
                    {
                        interactionPriorities[(int)InteractionID.Opponent] += 2f;
                    }
                    else
                    {
                        interactionPriorities[(int)InteractionID.Opponent] += 1f;
                    }
                    
                    // multiply interactionPriority with interactionWeight
                    interactionPriorities[(int)InteractionID.Opponent] *= interactionWeights[(int)InteractionID.Opponent];

                    // set the interactionGoal to the opponent
                    interactionGoals[(int)InteractionID.Opponent] = opponent.transform;

                    foundInteraction = true;
                }
            }
        }
    }

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
                }
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
            // cast this index zo an InteractionID
            InteractionID foundInteractionID = (InteractionID)highestPriorityIndex;
            
            if (foundInteractionID != interactionID) // TODO: only change id if according priority is a given amount higher than priority of old id
            {
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
