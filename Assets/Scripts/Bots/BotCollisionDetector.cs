using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotCollisionDetector : MonoBehaviour
{
    public event Action<Transform> OnOpponentBubbbleDetected;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        int bubbleTeamNumber = other.gameObject.GetComponent<Bubble>().GetTeamNumber();
        int botTeamNumber = transform.parent.GetComponent<Bot>().GetTeamNumber();
        if (bubbleTeamNumber != botTeamNumber)
        {
            Debug.Log("Opponent Bubble detected");
            OnOpponentBubbbleDetected?.Invoke(other.transform);
        }
    }
}
