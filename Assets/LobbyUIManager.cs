using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Mirror;

public class LobbyUIManager : NetworkBehaviour
{
    public static LobbyUIManager singleton { get; internal set; }

    public VisualTreeAsset playerListItemTemplate;

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
    Button rerollName;
    Label playersLabel;
    ScrollView playersList;

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

        // if there are no connections, then we are not ready
        if (connections.Count == 0)
        {
            allReady = false;
            return;
        }

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
            playButton.text = "Starting in " + Mathf.FloorToInt(Mathf.Max(0, countdown));
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
        rerollName = root.Q("reroll") as Button;
        playersLabel = root.Q("playersLabel") as Label;
        playersList = root.Q("playersList") as ScrollView;

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
            username.value = currentUsername;
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
        rerollName.clicked += ActionRerollName;

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
        playersList.contentContainer.Clear();
        foreach (BBNetworkManager.ConnectionInfo connection in connections)
        {
            VisualElement playerListItem = playerListItemTemplate.CloneTree();
            (playerListItem.Q("name") as Label).text = connection.username;
            if (connection.readyToBegin)
            {
                playerListItem.ElementAt(0).AddToClassList("ready");
            }
            playersList.contentContainer.Add(playerListItem);
        }
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

        username.SetEnabled(!newValue);
        duration.SetEnabled(!newValue);
        rerollName.style.display = newValue ? DisplayStyle.None : DisplayStyle.Flex;
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

    [ClientCallback]
    private void UpdateUsernameText()
    {
        // iterate over connections and find ours
        foreach (BBNetworkManager.ConnectionInfo connection in connections)
        {

            var serverConnectionId = NetworkClient.connection.identity.gameObject.GetComponent<EmptyPlayer>().serverConnectionId;

            if (connection.connectionId == serverConnectionId)
            {
                currentUsername = connection.username;
                if (username.panel.focusController.focusedElement != username)
                {
                    username.value = connection.username;
                }
                ready = connection.readyToBegin;
                break;
            }
        }
        playButton.text = ready ? "Waiting for others" : "Ready";
    }

    private void ActionRerollName()
    {
        var message = new BBNetworkManager.GetRandomName { };
        NetworkClient.Send(message);
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
