using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CaptureBubble : NetworkBehaviour
{
    // Start is called before the first frame update
    public CharacterBase player;
 
    // Update is called once per frame
    void Update()
    {
        if (!isServer) return;

        transform.position = player.transform.position;
    }
}
