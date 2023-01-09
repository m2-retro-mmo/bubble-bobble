using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private int pointsTeam0 = 0;
    private int pointsTeam1 = 0;

    UIDocument document;
    Label labelPointsTeam0;
    Label labelPointsTeam1;

    Label ping;
    Label playerName;
    VisualElement bubbleContainer;


    void OnEnable()
    {
        document = GetComponent<UIDocument>();

        if (document == null)
        {
            Debug.LogError("No document found");
        }

        // get team point labels
        VisualElement root = document.rootVisualElement;
        labelPointsTeam0 = root.Q("PointsTeam0") as Label;
        labelPointsTeam1 = root.Q("PointsTeam1") as Label;

        // ping label
        ping = root.Q("PingValue") as Label;
        playerName = root.Q("PlayerName") as Label;
        // get bubble container
        bubbleContainer = root.Q("BubbleContainer") as VisualElement;
    }

    // Start is called before the first frame update
    void Start()
    {
        SetPing(20);
        SetBubbleCount(3);
    }

    public void SetPlayerName(string name)
    {
        playerName.text = name;
    }

    public void SetBubbleCount(int count)
    {
        for (int i = 0; i < bubbleContainer.childCount; i++)
        {
            bubbleContainer.hierarchy.ElementAt(i).visible = (i < count);
        }
    }


    public void AddTeamPoint(int team)
    {
        switch (team)
        {
            case 0:
                pointsTeam0++;
                labelPointsTeam0.text = pointsTeam0.ToString();
                break;
            case 1:
                pointsTeam1++;
                labelPointsTeam1.text = pointsTeam1.ToString();
                break;
            default:
                Debug.LogWarning("Invalid Team given.");
                break;
        }
    }

    public void SetPing(int pingValue)
    {
        ping.text = pingValue.ToString() + "ms";
    }
}
