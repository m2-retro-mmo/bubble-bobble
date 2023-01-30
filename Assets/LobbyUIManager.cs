using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Mirror;

public class LobbyUIManager : NetworkBehaviour
{
    public static LobbyUIManager singleton { get; internal set; }

    // Synced Variables
    public readonly SyncList<BBNetworkManager.ConnectionInfo> connections = new SyncList<BBNetworkManager.ConnectionInfo>();
    [SyncVar(hook = nameof(OnGameDurationChanged))] public int gameDurationSeconds = 100;
    [SyncVar] public float countdown = 5f;
    [SyncVar(hook = nameof(OnReadyChanged))] bool allReady = false;

    // Document and UI Elements
    UIDocument document;
    TextField username;
    RadioButtonGroup duration;
    Button playButton;
    Button exitButton;
    Label playersLabel;

    // Client only
    bool ready = false;
    string currentUsername = "";


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
    }

    void Update()
    {
        if (isClient && allReady)
        {
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

        connections.Callback += OnConnectionsChanged;
    }

    public override void OnStartServer()
    {
        var instance = BBNetworkManager.singleton as BBNetworkManager;
        UpdateFromServer(instance.connectionRefs, instance.gameDuration);
        SetupUI();
    }
    public override void OnStartClient()
    {
        SetupUI();
    }

    private void SetupUI()
    {
        // Username Setup
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

        playButton.clicked += ActionPlay;
        exitButton.clicked += ActionDisconnect;

        GetPlayerLobbyList();
        UpdateUsernameText();
    }

    void OnConnectionsChanged(SyncList<BBNetworkManager.ConnectionInfo>.Operation op, int index, BBNetworkManager.ConnectionInfo oldItem, BBNetworkManager.ConnectionInfo newItem)
    {
        GetPlayerLobbyList();
        UpdateUsernameText();
    }

    private void GetPlayerLobbyList()
    {
        string playerList = "";
        foreach (BBNetworkManager.ConnectionInfo connection in connections)
        {
            playerList += connection.username + " " + (connection.readyToBegin ? "(ready)" : "(not ready)") + "\n";
        }
        playersLabel.text = playerList;
    }

    private void OnGameDurationChanged(int oldValue, int newValue)
    {
        duration.value = durationToIndex(newValue);
    }

    private void OnReadyChanged(bool oldValue, bool newValue)
    {
        if (!newValue)
        {
            playButton.text = ready ? "Waiting for others" : "Ready";
        }
    }

    private void ActionPlay()
    {
        var message = new BBNetworkManager.ChangeReadyMessage { ready = !ready };
        NetworkClient.Send(message);
    }
    private void ActionDisconnect()
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

    [Client]
    private void UpdateUsernameText()
    {
        if (!NetworkClient.active) return;
        // iterate over connections and find ours
        foreach (BBNetworkManager.ConnectionInfo connection in connections)
        {

            var serverConnectionId = NetworkClient.connection.identity.gameObject.GetComponent<EmptyPlayer>().serverConnectionId;

            if (connection.connectionId == serverConnectionId)
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
            case 5:
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
                return 5;
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
}
