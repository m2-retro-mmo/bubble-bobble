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

    [Server]
    public void collect()
    {
        Debug.Log("Collected");
        // Destroy(gameObject, 1f);
        // TODO: should take the diamond, instead of destroying itas
        if (!collected)
        {
            Destroy(gameObject, 1f);
            collected = true;
        }
    }
}
