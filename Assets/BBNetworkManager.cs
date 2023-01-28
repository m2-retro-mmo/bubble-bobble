using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class BBNetworkManager : NetworkManager
{

    public struct ConnectionInfo
    {
        public NetworkConnectionToClient conn;
        public int index;
        public string username;
        public bool readyToBegin;
    }

    [Header("Bubble Bobble Network Manager")]
    [Scene]
    public string RoomScene;
    [Scene]
    public string GameScene;

    public int gameDuration = 60;

    private bool gameRunning = false;

    private List<ConnectionInfo> connectionRefs = new List<ConnectionInfo>();

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        ServerChangeScene(RoomScene);
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);

        if (!gameRunning) return;

        GameManager.singleton.CreatePlayer(conn);
    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);

        // check if connection is already in connectionRefs
        for (int i = 0; i < connectionRefs.Count; i++)
        {
            if (connectionRefs[i].conn == conn)
            {
                return;
            }
        }

        ConnectionInfo newConnectionInfo = new ConnectionInfo();
        newConnectionInfo.conn = conn;
        newConnectionInfo.username = "Player " + conn.connectionId;
        connectionRefs.Add(newConnectionInfo);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.identity != null)
        {

            GameManager.singleton.RemovePlayer(conn);

            // iterate over connectionRefs and remove the one that matches conn
            for (int i = 0; i < connectionRefs.Count; i++)
            {
                if (connectionRefs[i].conn == conn)
                {
                    connectionRefs.RemoveAt(i);
                    break;
                }
            }
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
}
