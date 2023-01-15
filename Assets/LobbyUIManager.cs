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

    public static LobbyUIManager singleton { get; internal set; }

    bool InitializeSingleton()
    {
        if (singleton != null && singleton == this)
            return true;

        if (singleton != null)
        {
            Debug.LogError("Multiple GameManagers in the scene");
            return false;
        }

        singleton = this;
        return true;
    }

    void Start()
    {
        InitializeSingleton();
    }

    void OnDestroy()
    {
        if (singleton == this)
            singleton = null;
    }

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
        (BBNetworkManager.singleton as BBNetworkManager).OnRoomServerPlayersReady();
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
