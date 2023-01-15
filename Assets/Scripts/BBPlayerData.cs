using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BBPlayerData : NetworkBehaviour
{
    [SyncVar] public int index;
    [SyncVar] public string username;
    [SyncVar] public bool readyToBegin;

    public void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
