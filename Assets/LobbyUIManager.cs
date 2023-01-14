using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UIElements;

public class LobbyUIManager : MonoBehaviour
{
    UIDocument document;

    TextField playerName;
    TextField duration;

    VisualElement playersContainer;

    Button playButton;
    Button exitButton;

    void OnEnable()
    {
        document = GetComponent<UIDocument>();

        if (document == null)
        {
            Debug.LogError("No document found");
        }

        VisualElement root = document.rootVisualElement;
        playersContainer = root.Q("ListOfPlayers") as VisualElement;
        playerName = root.Q("PlayerName") as TextField;
        duration = root.Q("DurationValue") as TextField;
        // TODO Set duration only editable for host
        
        playButton = root.Q("PlayButton") as Button;
        // TODO remove button if player is not host, playButton.Clear();
        playButton.clicked += Play;

        exitButton = root.Q("ExitButton") as Button;
        exitButton.clicked += Quit;
    }

    private void Play()
    {
        // TODO set player name in game + duration
        Debug.Log("Player name: " + playerName.text + ", Game duration: " + duration.text);
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
