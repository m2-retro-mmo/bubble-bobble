using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCaptureBubble : MonoBehaviour
{
    // Start is called before the first frame update
    public TestCharacterBase player;
 
    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position;
    }
}
