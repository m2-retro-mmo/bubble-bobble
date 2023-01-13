using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BBNetworkManager : NetworkManager
{

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        // you can send the message here, or wherever else you want
        CreatePlayerMessage message = new CreatePlayerMessage();

        NetworkClient.Send(message);
    }
}

public struct CreatePlayerMessage : NetworkMessage
{ }
