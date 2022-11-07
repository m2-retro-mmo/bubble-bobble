using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hort : MonoBehaviour
{
    public int diamonds = 0;
    public byte team = 0;

    public void AddDiamond()
    {
        diamonds++;
        Debug.Log(diamonds);
    }
}
