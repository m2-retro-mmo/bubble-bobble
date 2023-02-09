using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    UIDocument document;
    VisualElement root;
    Label labelPointsTeam0;
    Label labelPointsTeam1;

    Label ping;
    Label playerName;
    Label duration;

    VisualElement bubbleIndicator;
    [SerializeField]
    Sprite emptyBubbleSprite;
    [SerializeField]
    Sprite purpleBubbleSprite;
    [SerializeField]
    Sprite orangeBubbleSprite;

    StyleBackground emptyBubble;
    StyleBackground orangeBubble;
    StyleBackground purpleBubble;

    bool isPurpleTeam = true;

    void OnEnable()
    {
        document = GetComponent<UIDocument>();

        if (document == null)
        {
            Debug.LogError("No document found");
        }

        // get team point labels
        root = document.rootVisualElement;
        labelPointsTeam0 = root.Q("orangeScore") as Label;
        labelPointsTeam1 = root.Q("purpleScore") as Label;

        // get bubble container
        bubbleIndicator = root.Q("bubbleIndicator") as VisualElement;
        emptyBubble = new StyleBackground(emptyBubbleSprite);
        orangeBubble = new StyleBackground(orangeBubbleSprite);
        purpleBubble = new StyleBackground(purpleBubbleSprite);

        // duration label
        duration = root.Q("timer") as Label;

        // show menu
        root.style.display = DisplayStyle.Flex;
    }

    // Start is called before the first frame update
    void Start()
    {
        SetBubbleCount(3);
    }

    public void SetPlayerName(string name)
    {
        playerName.text = name;
    }

    public void SetBubbleColorOrange()
    {
        isPurpleTeam = false;
        for (int i = 0; i < bubbleIndicator.childCount; i++)
        {
            if (bubbleIndicator.hierarchy.ElementAt(i).style.backgroundImage != emptyBubble)
            {
                bubbleIndicator.hierarchy.ElementAt(i).style.backgroundImage = orangeBubble;
            }
        }
    }

    public void SetBubbleCount(int count)
    {
        for (int i = 0; i < bubbleIndicator.childCount; i++)
        {
            bubbleIndicator.hierarchy.ElementAt(i).style.backgroundImage = (i < count) ? (isPurpleTeam ? purpleBubble : orangeBubble) : emptyBubble;
        }
    }

    public void UpdateTeamPoints(int team, int points)
    {
        switch (team)
        {
            case 0:
                // pad with zero so there are always two digits
                labelPointsTeam0.text = points.ToString("00");
                break;
            case 1:
                labelPointsTeam1.text = points.ToString("00");
                break;
            default:
                Debug.LogWarning("Invalid Team given.");
                break;
        }
    }

    public void SetDuration(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        duration.text = string.Format("{00:00}:{01:00}", minutes, seconds);
    }

    public void hideMenu()
    {
        root.style.display = DisplayStyle.None;
    }
}
