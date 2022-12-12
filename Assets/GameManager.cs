using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;

/**
This class handles:
- Player initalization
- Hort creation
- Bot initialzation 
*/
public class GameManager : NetworkBehaviour
{

    public Player playerPrefab;
    public Hort hortPrefab;
    public Map map;
    public Camera cam;
    public Camera minimapCam;

    [SerializeField]
    [Tooltip("true if the game should run with bots")]
    private bool startGameWithBots;

    [SerializeField]
    [Tooltip("The prefab of the bot")]
    private Bot botPrefab;

    [SerializeField]
    [Tooltip("The number of bots the game should start with")]
    private int botNumber;

    private void CreatePlayer(NetworkConnectionToClient conn, CreatePlayerMessage message)
    {
        foreach (Player player in FindObjectsOfType<Player>())
        {
            if (player.connectionToClient == conn)
            {
                Debug.Log("Player already exists");
                return;
            }
        }

        Player p = Instantiate(playerPrefab, new Vector3(((float)22) + 0.5f, ((float)22) + 0.5f, 0), Quaternion.identity);
        map.PlaceCharacter(p);
        p.SetTeamNumber(1);
        NetworkServer.AddPlayerForConnection(conn, p.gameObject);
    }

    // Start is called before the first frame update
    [Server]
    void Start()
    {
        if (isServerOnly)
        {
            cam = GameObject.Find("Server Camera").GetComponent<Camera>();
            cam.enabled = true;
            cam.GetComponent<Camera>().enabled = true;
            cam.GetComponent<AudioListener>().enabled = true;
        }

        map.NewMap();

        // get all connections and instanciate a player for each connection
        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn != null)
            {
                CreatePlayer(conn, new CreatePlayerMessage());
            }
        }

        // minimap camera should follow main player
        // CameraFollow minimapCamFollow = minimapCam.GetComponent<CameraFollow>();
        // minimapCamFollow.target = player.transform;

        if (startGameWithBots)
        {
            GameObject bots = new GameObject("Bots");

            Graph graph = new Graph(map, true);

            for (int i = 0; i < botNumber; i++)
            {
                // spawn a bot
                // TODO: spawn the bot within the bounds of the map
                Bot bot = Instantiate(botPrefab, new Vector3(((float)22) + 0.5f, ((float)22) + 0.5f, 0), Quaternion.identity);
                bot.transform.parent = bots.transform;

                bot.SetTeamNumber(0); // TODO set team number randomly
                map.PlaceCharacter(bot);

                bot.GetComponent<BotMovement>().SetGraph(graph);
                bot.StartBot();
            }
        }

        // register connection handler function
        NetworkServer.RegisterHandler<CreatePlayerMessage>(CreatePlayer);
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: if new player joined place player on the map
    }
}
