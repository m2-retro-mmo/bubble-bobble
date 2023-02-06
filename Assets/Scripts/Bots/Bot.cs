using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    private float interactionRadius = 500;

    private Vector3 botPosition;

    private Collider2D[] interactionColliders;

    private BotController botController;

    private float prevPriorityValue = 0;

    private bool DEBUG_BOTS;

    private const float PRIORITY_THRESHOLD = 1f;

    private const float REFRESH_RATE_GOAL = 10f;

    private const float REFRESH_RATE_BUBBLE = 0.5f;

    [SerializeField]
    private string BASE_URL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSfonsU3CpscyOGBVfnnDrQxmsMqeSotW3s0gT5KUHWWzRuYnQ/formResponse";

    public override void Start()
    {
        base.Start();

        botController = GetComponent<BotController>();

        hort = GameObject.FindGameObjectsWithTag("Hort").Where(x => x.GetComponent<Hort>().team == teamNumber).FirstOrDefault().transform;

        DEBUG_BOTS = gameManager.GetDebugBots();

        StartBot();
    }

    /// <summary>
    /// starts the bot including the coroutines for checking the area for interactions and checking for bubbles
    /// </summary>
    public void StartBot()
    {
        //StartCoroutine(CheckAreaOfInterest());
        //StartCoroutine(CheckForBubbles());
    }

    /// <summary>
    /// for resetting the bot 
    /// stops all coroutines and resets the interaction id
    /// invokes the start bot method after the given time
    /// </summary>
    /// <param name="restartTime">seconds to wait until the bbot will be restarted in seconds</param>
    public void ResetBot(float restartTime)
    {
        prevPriorityValue = 0;
        StopAllCoroutines();
        CancelInvoke("StartBot");
        SetInteractionID(InteractionID.None);
        SetChangedInteractionID(false);

        Invoke("StartBot", restartTime);
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
    /// Checks for bubbles of opponents in the area around the player.
    /// </summary>
    private void CheckForOpponentBubbles()
    {
        // get all opponent bubbles in the area 
        Collider2D[] opponentBubbleColliders = GetCollidersByTag("Bubble").Where(b => b.GetComponent<Bubble>().GetTeamNumber() == GetOpponentTeamNumber(teamNumber)).ToArray();

        if (opponentBubbleColliders.Length > 0)
        {
            foreach (Collider2D bubble in opponentBubbleColliders)
            {
                if (!bubble.GetComponent<Bubble>().GetAvoidedByBot())
                {
                    detectedBubble = true;
                    bubble.GetComponent<Bubble>().SetAvoidedByBot(detectedBubble);
                    botController.SetGoal(bubble.transform);
                    if (DEBUG_BOTS)
                    {
                        Debug.Log("set goal to bubble");
                    }
                }
            }
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
        Collider2D[] opponentWithDiamondColliders = opponentColliders.Where(c => c.gameObject.GetComponent<CharacterBase>().GetHoldsDiamond() == true).ToArray();

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
            int index = 0;
            // shuffle the opponents array to get a random opponent
            opponentColliders = RandomizeGoalArray(opponentColliders);

            // loop through all opponents 
            while (index < opponentColliders.Length)
            {
                // // add a random value to the index to get a random opponent
                // // this is done to avoid that all the bots always targets the same opponent
                // index += Random.Range(0, 5);

                CharacterBase opponent = opponentColliders[index].gameObject.GetComponent<CharacterBase>();

                // check if the opponent is not captured
                if (!opponent.GetIsCaptured())
                {
                    ApplyInteractionPriority(InteractionID.Opponent, opponentPriority, opponent.transform.Find("Shape").transform);
                    break; // break the loop if an opponent is found
                }
                index++;
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
            int index = 0;
            // shuffle the teammates array to get a random teammate
            teammateColliders = RandomizeGoalArray(teammateColliders);
            
            // loop through all teammates until a teammate is found who is captured
            while (index < teammateColliders.Length)
            {
                CharacterBase teammate = teammateColliders[index].gameObject.GetComponent<CharacterBase>();

                // check if the teammate is captured - if he is, free teammate
                if (teammate.GetIsCaptured())
                {
                    ApplyInteractionPriority(InteractionID.Teammate, 1f, teammate.transform);
                    break; // break the loop if a teammate is found
                }
                index++;
            }
        }
    }

    /// <summary>
    /// Checks for diamonds in the area around the bot if the bot does not hold a diamond and sets the interaction priorities accordingly
    /// </summary>
    private void CheckForDiamond()
    {
        if (GetHoldsDiamond())
        {
            // if the bot holds a diamond, the priority for the hort is higher
            interactionPriorities[(int)InteractionID.Hort] += 1f;
            return;
        }

        // get all diamonds in the area
        Collider2D[] diamondColliders = GetCollidersByTag("Diamond");

        // are there diamonds near by
        if (diamondColliders.Length > 0)
        {
            diamondColliders = RandomizeGoalArray(diamondColliders, 3);

            // get the first diamond
            Diamond diamond = diamondColliders[0].gameObject.GetComponent<Diamond>();
            ApplyInteractionPriority(InteractionID.Diamond, 1f, diamond.transform);
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
            ApplyInteractionPriority(InteractionID.Hort, 1f, hort);
        }
    }

    /// <summary>
    /// The function sets the interactionPriority for the given interaction to the given goal
    /// </summary>
    /// <param name="InteractionID">The ID of the interaction.</param>
    /// <param name="Transform">The transform of the object that the interaction is being performed
    /// on.</param>
    private void ApplyInteractionPriority(InteractionID id, float priority, Transform goal)
    {
        // increment the interactionPriority for this interaction
        interactionPriorities[(int)id] += priority;

        // multiply interactionPriority with interactionWeight 
        interactionPriorities[(int)id] *= interactionWeights[(int)id];

        // set the interactionGoal to to the given goal
        interactionGoals[(int)id] = goal;

        // set foundInteraction to true if the interactionPriority for this intercation is higher than 0
        if (interactionPriorities[(int)id] > 0)
        {
            foundInteraction = true;
        }
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
                if (DEBUG_BOTS)
                {
                    Debug.Log("all interaction priorities: \n" +
                        "Opponent: " + interactionPriorities[0] + " \n" +
                        "Teammate: " + interactionPriorities[1] + " \n" +
                        "Diamond: " + interactionPriorities[2] + " \n" +
                        "Hort: " + interactionPriorities[3] + " \n" +
                        "Item: " + interactionPriorities[4]);

                    Debug.Log("set goal to: " + foundInteractionID);
                }
                changedInteractionID = true;
                interactionID = foundInteractionID;
                // set goal of bot movement to goal position
                botController.SetGoal(interactionGoals[highestPriorityIndex]);
            }
        }
        else // if no interaction was found, increase the radius
        {
            interactionRadius += 50f;
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
            (c.gameObject.CompareTag("Player") ||
            c.gameObject.CompareTag("Bot")) &&
            (c.TryGetComponent(out CharacterBase characterBase)
            && characterBase.GetTeamNumber() == teamNumber)).ToArray();

        // order by distance
        colliders = colliders.OrderBy(c => Vector3.Distance(botPosition, c.transform.position)).ToArray();

        return colliders;
    }

    /// <summary>
    /// It takes an array of colliders, shuffles the first half of the array, and returns the array
    /// </summary>
    /// <param name="colliders">The array of colliders to randomize</param>
    /// <param name="arrayPart">The part of the array that should be randomized</param>
    /// <returns>
    /// The colliders array is being returned.
    /// </returns>
    public Collider2D[] RandomizeGoalArray(Collider2D[] colliders, int arrayPart = 2)
    {
        if (colliders.Length > 0)
        {
            // Shuffle the first half of the array to randomize the order
            // This is done to prevent bots from always choosing the same target
            int halfLength = colliders.Length / arrayPart;
            for (int i = 0; i < halfLength; i++)
            {
                int randomIndex = Random.Range(i, halfLength);
                Collider2D temp = colliders[i];
                colliders[i] = colliders[randomIndex];
                colliders[randomIndex] = temp;
            }
        }
        return colliders;
    }

    /// <summary>
    /// this method is called when a new bot is created and sets the interaction weights to random values
    /// </summary>
    public void RandomizeInteractionWeights()
    {
        System.Random rand = new System.Random();
        for (int i = 0; i < interactionWeights.Length; i++)
        {
            // set random weight between 1 and 10
            int weight = rand.Next(1, 11);
            interactionWeights[i] = weight;
        }
    }


    /// <summary>
    /// The function prepares the data to be sent to the google form
    /// tracking data like the interaction weights, the number of collected  diamonds, captured and uncaptured characters
    /// </summary>
    public void Send()
    {
        // get a bool if this bot was in the winning team
        bool teamWon = gameManager.GetWinnerForTracking() == teamNumber;
        // create a list of strings to send to the google form that holds the data
        List<string> data = new List<string>();
        // add the interaction weights to the list
        foreach (float i in interactionWeights)
        {
            data.Add(i.ToString());
        }
        // add the number of collected diamonds, captured and uncaptured characters
        data.Add(GetDiamondCounter().ToString());
        data.Add(botController.GetOpponentCapturedCounter().ToString());
        data.Add(GetUncapturedCounter().ToString());
        data.Add(teamWon.ToString());
        StartCoroutine(Post(data));
    }

    /// <summary>
    /// It takes a list of strings, adds them to a WWWForm, and then sends them to a Google Form (data is exported to a google sheet from the google form)
    /// </summary>
    /// <param name="data">A list of strings that contains the data to be sent to the Google Form.</param>
    IEnumerator Post(List<string> data)
    {
        // the entry ids of the answer fields in the google form
        string[] entryFields = { "entry.1555711059", "entry.2141082333", "entry.886997526", "entry.1317052357", "entry.299362154", "entry.1696318452", "entry.822962180", "entry.304877644", "entry.2077812130" };
        WWWForm form = new WWWForm();
        for (int i = 0; i < data.Count; i++)
        {
            form.AddField(entryFields[i], data[i]);
        }
        byte[] rawData = form.data;
        // the url of the google form
        WWW www = new WWW(BASE_URL, rawData);
        yield return www;
    }

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

    public bool GetDetectedBubble()
    {
        return detectedBubble;
    }

    public void SetDetectedBubble(bool value)
    {
        detectedBubble = value;
    }
}
