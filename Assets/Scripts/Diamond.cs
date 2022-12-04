using UnityEngine;
using Mirror;

public class Diamond : NetworkBehaviour
{
    [Server]
    public void collect()
    {
        Debug.Log("Collected");
        // TODO: maybe play an animation here
        Destroy(gameObject, 0);
    }
}
