using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UIElements;

public class LobbyUIManager : MonoBehaviour
{
    UIDocument document;

    Label playerName;
    Label duration;

    VisualElement playersContainer;

    Button playButton;
    Button exitButton;

    private UIManager uIManager;

    void OnEnable()
    {
        document = GetComponent<UIDocument>();

        if (document == null)
        {
            Debug.LogError("No document found");
        }

        VisualElement root = document.rootVisualElement;
        playersContainer = root.Q("ListOfPlayers") as VisualElement;
        playerName = root.Q("PlayerName") as Label;
        duration = root.Q("DurationValue") as Label;
        playButton = root.Q("PlayButton") as Button;
        playButton.clicked += Play;
        exitButton = root.Q("ExitButton") as Button;
        exitButton.clicked += Quit;
    }

    private void Play()
    {
        // TODO set player name in game + duration
        SceneManager.LoadSceneAsync("Main");
    }

    private static void Quit()
    {
        #if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
        #else
            Application.Quit();
        #endif
    }
}
