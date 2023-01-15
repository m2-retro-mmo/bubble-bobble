using UnityEngine;
using Mirror;
using System;

public class Diamond : NetworkBehaviour
{
    [SyncVar]
    private bool collected = false;

    public bool GetCollected()
    {
        return collected;
    }

    public void Drop()
    {
        collected = false;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.LogWarning(other.collider.name);
    }

    [Server]
    public void Collect()
    {
        this.collected = true;
        // TODO: should take the diamond, instead of destroying itas
        Destroy(gameObject);
    }
}
