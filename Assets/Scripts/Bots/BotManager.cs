//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Tilemaps;

//public class BotManager : MonoBehaviour
//{
//    [SerializeField]
//    [Tooltip("true if the game should run with bots")]
//    private bool startGameWithBots;

//    [SerializeField]
//    private bool DEBUG = false;

//    [SerializeField]
//    [Tooltip("The number of bots the game should start with")]
//    private int botNumber;

//    [SerializeField]
//    [Tooltip("The prefab of the bot")]
//    private GameObject botPrefab;

//    private Tilemap obstacle_tilemap;
    
//    private Tilemap ground_tilemap;

//    private Graph graph;

//    // Start is called before the first frame update
//    void Start()
//    {
//        if (startGameWithBots)
//        {
//            GameObject bots = new GameObject("Bots");
            
//            obstacle_tilemap = GameObject.Find("Obstacles").GetComponent<Tilemap>();
//            ground_tilemap = GameObject.Find("Ground").GetComponent<Tilemap>();

//            graph = new Graph(obstacle_tilemap, ground_tilemap, true);
            
//            if (!DEBUG)
//            {
//                for (int i = 0; i < botNumber; i++)
//                {
//                    // spawn a bot
//                    // TODO: spawn the bot within the bounds of the map
//                    GameObject bot = Instantiate(botPrefab, new Vector3(Random.Range(-39, 10), Random.Range(-4, 23), 0), Quaternion.identity);
//                    bot.transform.parent = bots.transform;

//                    //TODO SetTeamNumber

//                    bot.GetComponent<BotMovement>().SetGraph(graph);
//                }
//            }
//            else
//            {
//                GameObject bot = Instantiate(botPrefab, new Vector3(-21.5f, 19f, 0f), Quaternion.identity);
//                bot.GetComponent<BotMovement>().SetGraph(graph);
//            }
//        }
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}
