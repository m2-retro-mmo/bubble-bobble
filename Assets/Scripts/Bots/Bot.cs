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
    Diamond = 2,
    Hort = 3,
    Item = 4,
    None = 5
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

    [HideInInspector]
    public bool detectedBubble = false;

    // the weights of the interactions
    private float[] interactionWeights = new float[] { 2, 5, 4, 3, 1 }; 

    private float[] interactionPriorities;

    private Transform[] interactionGoals;

    // area radius around the bot
    private float interactionRadius = 10;

    private Vector3 botPosition;

    private Collider2D[] interactionColliders;

    private BotMovement botMovement;

    private float prevPriorityValue = 0;

    private Transform hort;

    private const float PRIORITY_THRESHOLD = 1f;

    private const float REFRESH_RATE_GOAL = 10f;

    private const float REFRESH_RATE_BUBBLE = 0.5f;

    public void Awake()
    {
        botMovement = GetComponent<BotMovement>();
    }
    [Server]
    public override void Start()
    {
        base.Start();
        teamNumber = 0; // TODO: sp�ter anders l�sen, nur zum testen
        Debug.Log("bot diamond: " + GetHoldsDiamond().ToString());

        hort = GameObject.FindGameObjectsWithTag("Hort").Where(x => x.GetComponent<Hort>().team == teamNumber).FirstOrDefault().transform;
    }

    public void ResetBot(float restartTime)
    {
        prevPriorityValue = 0;
        StopAllCoroutines();
        SetInteractionID(InteractionID.None);
        SetChangedInteractionID(false);

        Invoke("StartBot", restartTime);
    }

    public void StartBot()
    {
        //StartCoroutine(CheckAreaOfInterest());
        StartCoroutine(CheckForBubbles());
    }

    IEnumerator CheckForBubbles()
    {
        while (true)
        {
            botPosition = transform.position;
            // get all colliders in a radius around the bot
            interactionColliders = Physics2D.OverlapCircleAll(botPosition, interactionRadius);

            CheckForOpponentBubbles();

            yield return new WaitForSeconds(REFRESH_RATE_BUBBLE);
        }
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
            //Debug.Log("start searching for interaction...");
            foundInteraction = false;
            // reset all priority values
            interactionPriorities = new float[5];
            interactionGoals = new Transform[5];

            botPosition = transform.position;
            // get all colliders in a radius around the bot
            interactionColliders = Physics2D.OverlapCircleAll(botPosition, interactionRadius);

            CheckForOpponents();
            CheckForTeammates();
            CheckForDiamond();
            CheckForHort();

            CalculateInteractionID();

            //Debug.Log("found Interaction " + foundInteraction);
            //Debug.Log("changedInteraction " + changedInteractionID);
            //Debug.Log("interactionID: " + interactionID);

            yield return new WaitForSeconds(REFRESH_RATE_GOAL);
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
            //Debug.Log("opponent with diamond found");
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
                if (!opponent.GetIsCaptured())
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
        if (teammateColliders.Length > 0)
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

    /// <summary>
    /// Checks for bubbles of opponents in the area around the player.
    /// </summary>
    private void CheckForOpponentBubbles()
    {
        // get all opponent bubbles in the area 
        Collider2D[] opponentBubbleColliders = GetCollidersByTag("Bubble").Where(b => b.GetComponent<Bubble>().GetTeamNumber() == GetOpponentTeamNumber(teamNumber)).ToArray();

        if (opponentBubbleColliders.Length > 0)
        {
            Debug.Log("detected bbubbble");
            foreach (Collider2D bubble in opponentBubbleColliders)
            {
                detectedBubble = true;
                botMovement.SetGoal(bubble.transform);
                Debug.Log("set goal to bubble");
            }
        }
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
            if (GetHoldsDiamond())
            {
                interactionPriorities[(int)InteractionID.Hort] += 1f;
                return;
            }

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
                break;
            }
        }
    }

    /// <summary>
    /// Checks for bot's hort if the bot holds a diamond and ets the interaction priorities
    /// </summary>
    private void CheckForHort()
    {
        // only check for hort if the bot holds a diamond
        if (GetHoldsDiamond() && hort != null)
        {
            // has a higher priority to drop diamond
            interactionPriorities[(int)InteractionID.Hort] += 1f;

            // multiply interactionPriority with interactionWeight
            interactionPriorities[(int)InteractionID.Hort] *= interactionWeights[(int)InteractionID.Hort];

            // set the interactionGoal to hort
            interactionGoals[(int)InteractionID.Hort] = hort;

            if (interactionPriorities[(int)InteractionID.Hort] > 0)
            {
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
        Collider2D[] colliders = interactionColliders.Where(c => c.gameObject.tag == tagName).ToArray();
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
        Collider2D[] colliders = interactionColliders.Where(c =>
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
            // get highest value in interactionPriorities
            float priorityValue = interactionPriorities.Max();
            // get positive difference between priorityValue and prevPriorityValue
            float priorityDifference = Mathf.Abs(priorityValue - prevPriorityValue);
            prevPriorityValue = priorityValue;

            // get index of highest value in priority array
            int highestPriorityIndex = interactionPriorities.ToList().IndexOf(interactionPriorities.Max());
            // cast this index to an InteractionID
            InteractionID foundInteractionID = (InteractionID)highestPriorityIndex;

            // check: did we find a new interactionId that is higher prioritized the old interaction
            if (foundInteractionID != interactionID && priorityDifference >= PRIORITY_THRESHOLD) // only change id if according priority is a given amount higher than priority of old id
            {
                Debug.Log("all interaction priorities: \n" +
                    "Opponent: " + interactionPriorities[0] + " \n" +
                    "Teammate: " + interactionPriorities[1] + " \n" +
                    "Diamond: " + interactionPriorities[2] + " \n" +
                    "Hort: " + interactionPriorities[3] + " \n" +
                    "Item: " + interactionPriorities[4]);

                Debug.Log("set goal to: " + foundInteractionID);
                changedInteractionID = true;
                interactionID = foundInteractionID;
                // set goal of bot movement to goal position
                botMovement.SetGoal(interactionGoals[highestPriorityIndex]);
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

    public Transform GetHort()
    {
        return hort;
    }

    public bool GetDetectedBubble()
    {
        return detectedBubble;
    }

    public void SetDetectedBubble(bool value)
    {
        detectedBubble = value;
    }
}
