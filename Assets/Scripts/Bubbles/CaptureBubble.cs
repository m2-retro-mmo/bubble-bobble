using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CaptureBubble : NetworkBehaviour
{
    public CharacterBase player;

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position;
    }
}
