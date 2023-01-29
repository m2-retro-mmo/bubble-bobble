using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Mirror;

public class LobbyUIManager : NetworkBehaviour
{
    public readonly SyncList<BBNetworkManager.ConnectionInfo> connections = new SyncList<BBNetworkManager.ConnectionInfo>();
    [SyncVar(hook = nameof(OnGameDurationChanged))] public int gameDurationSeconds = 100;
    [SyncVar] public float countdown = 5f;
    [SyncVar (hook = nameof(OnReadyChanged))] bool allReady = false;

    UIDocument document;

    TextField username;
    RadioButtonGroup duration;

    Button playButton;
    Button exitButton;

    Label playersLabel;

    bool ready = false;
    string currentUsername = "";

    public static LobbyUIManager singleton { get; internal set; }


    [Server]
    public void UpdateFromServer(List<BBNetworkManager.ConnectionInfo> connections, int gameDurationSeconds)
    {
        this.connections.Clear();
        this.connections.AddRange(connections);
        this.gameDurationSeconds = gameDurationSeconds;
        countdown = 5f;

        // check if all players are ready
        bool allReady_ = true;
        foreach (var connection in connections)
        {
            if (!connection.readyToBegin)
            {
                allReady_ = false;
                break;
            }
        }
        allReady = allReady_;
        Debug.Log("allReady: " + allReady);
    }

    void Update()
    {
        if (isClient && allReady) {
            playButton.text = "Starting in " + Mathf.FloorToInt(countdown);
        }

        if (!isServer) return;
        // if the list has more than two players, count down

        if (countdown > 0 && allReady)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0)
            {
                (BBNetworkManager.singleton as BBNetworkManager).OnRoomServerPlayersReady();
            }
        }
    }

    bool InitializeSingleton()
    {
        if (singleton != null && singleton == this)
            return true;

        if (singleton != null)
        {
            Debug.LogError("Multiple Lobby UIs in the scene");
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

        username = root.Q("username") as TextField;
        duration = root.Q("duration") as RadioButtonGroup;
        playButton = root.Q("playBtn") as Button;
        exitButton = root.Q("exitBtn") as Button;
        playersLabel = root.Q("playersLabel") as Label;

        playersLabel.text = GetPlayerLobbyList();

        // Username Setup
        UpdateUsernameText();
        username.RegisterValueChangedCallback(evt =>
        {
            if (currentUsername == evt.newValue) return;
            if (evt.newValue.Length < 2 || evt.newValue.Length > 25) return;
            
            var message = new BBNetworkManager.ChangeNameMessage { newName = evt.newValue };
            NetworkClient.Send(message);
        });
        username.RegisterCallback<FocusOutEvent>(evt =>
        {
            UpdateUsernameText();
        });

        // Duration Setup
        duration.value = durationToIndex(gameDurationSeconds);
        duration.RegisterValueChangedCallback(evt =>
        {
            var message = new BBNetworkManager.ChangeGameDurationMessage { newDuration = indexToDuration(evt.newValue) };
            NetworkClient.Send(message);
        });
        duration.RegisterCallback<FocusOutEvent>(evt =>
        {
            duration.value = durationToIndex(gameDurationSeconds);
        });

        playButton.clicked += () =>
        {
            var message = new BBNetworkManager.ChangeReadyMessage { ready = !ready };
            NetworkClient.Send(message);
        };

        exitButton.clicked += Disconnect;
    }

    public override void OnStartClient()
    {
        connections.Callback += OnConnectionsChanged;
        UpdateUsernameText();
    }

    void OnConnectionsChanged(SyncList<BBNetworkManager.ConnectionInfo>.Operation op, int index, BBNetworkManager.ConnectionInfo oldItem, BBNetworkManager.ConnectionInfo newItem)
    {
        playersLabel.text = GetPlayerLobbyList();
        UpdateUsernameText();
    }

    private string GetPlayerLobbyList()
    {
        string playerList = "";
        foreach (BBNetworkManager.ConnectionInfo connection in connections)
        {
            playerList += connection.username + " " + (connection.readyToBegin ? "(ready)" : "(not ready)") + "\n";
        }
        return playerList;
    }

    private void OnGameDurationChanged(int oldValue, int newValue)
    {
        duration.value = durationToIndex(newValue);
    }

    private void OnReadyChanged(bool oldValue, bool newValue)
    {
        if (!newValue) {
            playButton.text = ready ? "Waiting for others" : "Ready";
        }
    }

    private void Disconnect()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            BBNetworkManager.singleton.StopHost();
        }
        // stop client if client-only
        else if (NetworkClient.isConnected)
        {
            BBNetworkManager.singleton.StopClient();
        }
        // stop server if server-only
        else if (NetworkServer.active)
        {
            BBNetworkManager.singleton.StopServer();
        }
    }

    private void UpdateUsernameText()
    {
        // iterate over connections and find ours
        foreach (BBNetworkManager.ConnectionInfo connection in connections)
        {
            if (connection.identity == NetworkClient.connection.identity)
            {
                currentUsername = connection.username;
                username.value = connection.username;
                //if focused set cursor to end
                if (username.panel.focusController.focusedElement == username)
                {
                    username.SelectRange(username.value.Length, username.value.Length);
                }
                ready = connection.readyToBegin;
                break;
            }
        }
        playButton.text = ready ? "Waiting for others" : "Ready";
    }


    private int durationToIndex(int duration)
    {
        switch (duration)
        {
            case 30:
                return 0;
            case 60:
                return 1;
            case 120:
                return 2;
            case 180:
                return 3;
            default:
                return 0;
        }
    }

    private int indexToDuration(int index)
    {
        switch (index)
        {
            case 0:
                return 30;
            case 1:
                return 60;
            case 2:
                return 120;
            case 3:
                return 180;
            default:
                return 30;
        }
    }
}
