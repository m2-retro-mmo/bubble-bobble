using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diamond : MonoBehaviour
{
    public void collect()
    {
        // TODO: maybe play an animation here
        Destroy(gameObject, 0);
    }
}
