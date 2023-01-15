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
        public GameObject playerData;
    }

    [Header("Bubble Bobble Network Manager")]
    [Scene]
    public string RoomScene;
    [Scene]
    public string GameScene;

    public int gameDuration = 60;

    public BBPlayerData playerDataPrefab;

    private bool gameRunning = false;

    private List<ConnectionInfo> connectionRefs = new List<ConnectionInfo>();

    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkClient.RegisterPrefab(playerDataPrefab.gameObject);
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

        GameObject newRoomGameObject = Instantiate(playerDataPrefab.gameObject, Vector3.zero, Quaternion.identity);
        newRoomGameObject.GetComponent<BBPlayerData>().username = "Player " + conn.connectionId;
        NetworkServer.Spawn(newRoomGameObject, conn);

        ConnectionInfo newConnectionInfo;
        newConnectionInfo.conn = conn;
        newConnectionInfo.playerData = newRoomGameObject;
        connectionRefs.Add(newConnectionInfo);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.identity != null)
        {

            // iterate over connectionRefs and remove the one that matches conn
            for (int i = 0; i < connectionRefs.Count; i++)
            {
                if (connectionRefs[i].conn == conn)
                {
                    NetworkServer.Destroy(connectionRefs[i].playerData);
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

    public void returnToLobby() {
        gameRunning = false;
        ServerChangeScene(RoomScene);
    }

    public override void ServerChangeScene(string newSceneName)
    {
        base.ServerChangeScene(newSceneName);
    }
}
