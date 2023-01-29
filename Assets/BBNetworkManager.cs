using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class BBNetworkManager : NetworkManager
{

    public struct ConnectionInfo
    {
        public NetworkIdentity identity;
        public int index;
        public string username;
        public bool readyToBegin;
    }
    public struct ChangeNameMessage : NetworkMessage
    {
        public string newName;
    }

    public struct ChangeReadyMessage : NetworkMessage
    {
        public bool ready;
    }

    public struct StartGameMessage : NetworkMessage
    {
    }

    public struct ChangeGameDurationMessage : NetworkMessage
    {
        public int newDuration;
    }

    [Header("Bubble Bobble Network Manager")]
    [Scene]
    public string RoomScene;
    [Scene]
    public string GameScene;

    public int gameDuration = 60;

    private bool gameRunning = false;

    private List<ConnectionInfo> connectionRefs = new List<ConnectionInfo>();

    public GameObject emptyPlayerPrefab;

    public override void Start() {
        // if the current scene is the game scene, start a host game
        if (SceneManager.GetActiveScene().path == GameScene) {
            gameDuration = 3600;
            gameRunning = true;
            StartHost();
        }
    }

    public override void OnStartClient()
    {
        NetworkClient.RegisterPrefab(emptyPlayerPrefab.gameObject);
        base.OnStartClient();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        // register message handlers
        NetworkServer.RegisterHandler<ChangeNameMessage>(OnChangeNameMessage);
        NetworkServer.RegisterHandler<ChangeReadyMessage>(OnChangeReadyMessage);
        NetworkServer.RegisterHandler<StartGameMessage>(OnStartGameMessage);
        NetworkServer.RegisterHandler<ChangeGameDurationMessage>(OnChangeGameDurationMessage);

        if (!gameRunning) ServerChangeScene(RoomScene);
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);

    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);

        // check if connection is already in connectionRefs
        for (int i = 0; i < connectionRefs.Count; i++)
        {
            if (connectionRefs[i].identity == conn.identity)
            {
                return;
            }
        }

        if (!gameRunning)
        {
            GameObject newRoomGameObject = Instantiate(emptyPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);
            NetworkServer.AddPlayerForConnection(conn, newRoomGameObject);
        }
        else
        {
            GameManager.singleton.CreatePlayer(conn);
        }

        ConnectionInfo newConnectionInfo = new ConnectionInfo();
        newConnectionInfo.identity = conn.identity;
        newConnectionInfo.username = NameGenerator.GetRandomName();
        connectionRefs.Add(newConnectionInfo);
        OnConnectionUpdated();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.identity != null)
        {
            if (GameManager.singleton != null) {
                GameManager.singleton.RemovePlayer(conn);
            }

            // iterate over connectionRefs and remove the one that matches conn
            for (int i = 0; i < connectionRefs.Count; i++)
            {
                if (connectionRefs[i].identity == conn.identity)
                {
                    connectionRefs.RemoveAt(i);
                    break;
                }
            }
        OnConnectionUpdated();
        }
        base.OnServerDisconnect(conn);
    }

    public void OnRoomServerPlayersReady()
    {
        // all players are readyToBegin, start the game
        gameRunning = true;
        ServerChangeScene(GameScene);
    }

    public void returnToLobby()
    {
        gameRunning = false;
        ServerChangeScene(RoomScene);
    }

    public override void ServerChangeScene(string newSceneName)
    {
        base.ServerChangeScene(newSceneName);
    }

    public override void OnServerSceneChanged(string sceneName) {
        if (gameRunning)
        {
            OnConnectionUpdated();
        }

        base.OnServerSceneChanged(sceneName);
    }

    private void OnConnectionUpdated()
    {
        // update list in lobby
        var lobby = LobbyUIManager.singleton;
        if (lobby == null) return;

        lobby.UpdateFromServer(connectionRefs, gameDuration);
    }

    private void OnChangeNameMessage(NetworkConnection conn, ChangeNameMessage msg)
    {
        // iterate over connectionRefs and update the one that matches conn
        for (int i = 0; i < connectionRefs.Count; i++)
        {
            if (connectionRefs[i].identity == conn.identity)
            {
                var newConnectionInfo = connectionRefs[i];
                newConnectionInfo.username = msg.newName;
                connectionRefs[i] = newConnectionInfo;
                break;
            }
        }
        OnConnectionUpdated();
    }

    private void OnChangeReadyMessage(NetworkConnection conn, ChangeReadyMessage msg)
    {
        // iterate over connectionRefs and update the one that matches conn
        for (int i = 0; i < connectionRefs.Count; i++)
        {
            if (connectionRefs[i].identity == conn.identity)
            {
                var newConnectionInfo = connectionRefs[i];
                newConnectionInfo.readyToBegin = msg.ready;
                connectionRefs[i] = newConnectionInfo;
                break;
            }
        }
        OnConnectionUpdated();
    }

    private void OnStartGameMessage(NetworkConnection conn, StartGameMessage msg)
    {
        OnRoomServerPlayersReady();
    }

    private void OnChangeGameDurationMessage(NetworkConnection conn, ChangeGameDurationMessage msg)
    {
        gameDuration = msg.newDuration;
        var lobby = LobbyUIManager.singleton;
        if (lobby == null) return;
        lobby.gameDurationSeconds = gameDuration;
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        SceneManager.LoadScene("MainMenu");
    }

    public string getPlayerName(NetworkConnection conn)
    {
        // iterate over connectionRefs and update the one that matches conn
        for (int i = 0; i < connectionRefs.Count; i++)
        {
            if (connectionRefs[i].identity == conn.identity)
            {
                return connectionRefs[i].username;
            }
        }
        return "";
    }
}
