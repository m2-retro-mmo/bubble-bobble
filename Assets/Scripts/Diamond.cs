using UnityEngine;
using Mirror;

public class Diamond : NetworkBehaviour
{
    private bool collected = false;

    public bool GetCollected()
    {
        return collected;
    }

    public void drop()
    {
        collected = false;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.LogWarning(other.collider.name);
    }

    [Server]
    public void collect()
    {
        Debug.Log("Collected");
        // Destroy(gameObject, 1f);
        // TODO: should take the diamond, instead of destroying itas
        if (!collected)
        {
            Destroy(gameObject, 0f);
            collected = true;
        }
    }
}
