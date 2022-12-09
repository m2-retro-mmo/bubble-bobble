using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Init : MonoBehaviour
{
    public GameObject PlayerPrefab;

    // Start is called before the first frame update
    private Vector2 startPos;
    void Start()
    {
        startPos = new Vector2(-50, 2);
        for (int i = 0; i <= 100; i++)
        {
            Instantiate(PlayerPrefab, startPos, transform.rotation);
            if (startPos.y > 20)
            {
                startPos = new Vector2(startPos.x + 2, 2);
            }
            else
            {
                startPos += new Vector2(0, 2);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
