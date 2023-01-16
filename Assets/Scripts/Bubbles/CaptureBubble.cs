using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CaptureBubble : NetworkBehaviour
{
    public CharacterBase player;
    [SerializeField] private Sprite teamBSprite;

    private void Start()
    {
        // render the opposite CaptureBubble color of the captured player
        if (player.teamNumber == 1) {
            GetComponent<SpriteRenderer>().sprite = teamBSprite;
        }
    }

    void Update()
    {
        transform.position = player.transform.position;
    }
}
