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
    [SerializeField] Texture2D team0BannerSprite;
    [SerializeField] Texture2D team1BannerSprite;
    [SerializeField] Texture2D noWinnerSprite;

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

    public void SetBanner(Texture2D sprite)
    {
        banner.style.backgroundImage = sprite;
    }

    public void SetTitle(string name)
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

        if (team0Score == team1Score)
        {
            // tie game
            SetBanner(noWinnerSprite);
            SetTitle("Tie Game");
        }
        else
        {
            if (team0Score > team1Score)
            {
                SetTitle("Orange wins");
                SetBanner(team0BannerSprite);
            }
            else
            {
                SetTitle("Purple wins");
                SetBanner(team1BannerSprite);
            };
        }

        SetTeamScores(team0Score, team1Score);
    }
}
