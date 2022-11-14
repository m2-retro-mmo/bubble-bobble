using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("true if the game should run with bots")]
    private bool startGameWithBots;

    [SerializeField]
    [Tooltip("The number of bots the game should start with")]
    private int botNumber;

    [SerializeField]
    [Tooltip("The prefab of the bot")]
    private GameObject botPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (startGameWithBots)
        {
            for (int i = 0; i < botNumber; i++)
            {
                // spawn a bot
                GameObject bot = Instantiate(botPrefab, new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10)), Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
