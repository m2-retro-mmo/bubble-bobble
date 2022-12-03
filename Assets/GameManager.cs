using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/**
This class handles:
- Player initalization
- Hort creation
- Bot initialzation 
*/
public class GameManager : MonoBehaviour
{

    public Player playerPrefab;
    public Hort hortPrefab;
    public Map map;
    public Camera cam;

    [SerializeField]
    [Tooltip("true if the game should run with bots")]
    private bool startGameWithBots;

    [SerializeField]
    [Tooltip("The prefab of the bot")]
    private Bot botPrefab;

    [SerializeField]
    [Tooltip("The number of bots the game should start with")]
    private int botNumber;

    private byte teamCount = 2;

    // Start is called before the first frame update
    void Start()
    {
        // instanciate a Hort for each Team
        List<Hort> horts = new List<Hort>();
        for (byte teamNumber = 0; teamNumber < teamCount; teamNumber++)
        {
            Hort hort = Instantiate(hortPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            hort.init(teamNumber);
            horts.Add(hort);
        }
        map.GenerateMap(horts);

        // instanciate a local player
        Player player = Instantiate(playerPrefab, new Vector3(((float)22) + 0.5f, ((float)22) + 0.5f, 0), Quaternion.identity);
        player.cam = cam;
        player.SetTeamNumber(1);
        map.PlaceCharacter(player);

        // camera should follow main player
        CameraFollow camFollow = cam.GetComponent<CameraFollow>();
        camFollow.target = player.transform;

        if (startGameWithBots)
        {
            GameObject bots = new GameObject("Bots");

            Tilemap obstacle_tilemap = GameObject.Find("Obstacles").GetComponent<Tilemap>();
            Tilemap ground_tilemap = GameObject.Find("Ground").GetComponent<Tilemap>();

            Graph graph = new Graph(obstacle_tilemap, ground_tilemap, true);

            for (int i = 0; i < botNumber; i++)
            {
                // spawn a bot
                // TODO: spawn the bot within the bounds of the map
                Bot bot = Instantiate(botPrefab, new Vector3(((float)22) + 0.5f, ((float)22) + 0.5f, 0), Quaternion.identity);
                bot.transform.parent = bots.transform;

                bot.SetTeamNumber(0); // TODO set team number randomly
                map.PlaceCharacter(bot);

                bot.GetComponent<BotMovement>().SetGraph(graph);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: if new player joined place player on the map
    }
}
