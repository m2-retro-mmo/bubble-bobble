using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameOverUIManager : MonoBehaviour
{
    UIDocument document;
    VisualElement root;
    Label labelPointsTeam0;
    Label labelPointsTeam1;

    Label labelWinnerTeam;

    VisualElement banner;
    public Texture2D team0BannerSprite;

    void OnEnable()
    {
        document = GetComponent<UIDocument>();

        if (document == null)
        {
            Debug.LogError("No document found");
        }

        // get team point labels
        root = document.rootVisualElement;
        labelPointsTeam0 = root.Q("ScoreTeamOrange") as Label;
        labelPointsTeam1 = root.Q("ScoreTeamLila") as Label;

        // get Winner team label
        labelWinnerTeam = root.Q("WinnerTeamName") as Label;

        // get banner
        banner = root.Q("Banner") as VisualElement;

        // hide menu
        root.style.display = DisplayStyle.None;
    }

    public void SetWinnerTeamBanner(int team)
    {
        if (team == 0)
        {
            banner.style.backgroundImage = team0BannerSprite;
        }
    }

    public void SetWinnerTeamName(string name)
    {
        labelWinnerTeam.text = name;
    }

    public void SetTeamScores(int team0score, int team1score)
    {
        labelPointsTeam0.text = team0score.ToString("00");
        labelPointsTeam1.text = team1score.ToString("00");
    }

    public void DisplayGameOver(int team0Score, int team1Score)
    {
        // show menu
        root.style.display = DisplayStyle.Flex;

        // get winner team
        int winnerScore = 0;
        int winnerTeam;
        if (team0Score > team1Score)
        {
            winnerScore = team0Score;
            winnerTeam = 0;
        }
        else
        {
            winnerScore = team1Score;
            winnerTeam = 1;
        };

        // set labels
        SetWinnerTeamBanner(winnerTeam);
        SetTeamScores(team0Score, team1Score);
        if (winnerTeam == 0) SetWinnerTeamName("Orange wins");
        else SetWinnerTeamName("Purple wins");
    }
}
