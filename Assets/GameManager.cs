using UnityEngine;
using System.Collections.Generic;
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

    private UIManager uIManager;

    [SerializeField]
    [Tooltip("true if the game should run with bots")]
    private bool startGameWithBots;

    [SerializeField]
    [Tooltip("true if the bot should be spawned in the area of the player")]
    private bool DEBUG_BOTS = false;

    [SerializeField]
    [Tooltip("The prefab of the bot")]
    private Bot botPrefab;

    [SerializeField]
    [Tooltip("The number of bots the game should start with")]
    private int botNumber;

    [SerializeField]
    [Tooltip("The duration of a game")]
    private float gameDuration;
    private bool timerIsRunning;

    private List<Hort> horts;

    private GameObject bots;

    private Graph graph;

    private byte playerCounterTeam0 = 0;
    private byte playerCounterTeam1 = 0;

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

        Player p = Instantiate(playerPrefab, new Vector3(((float)22), ((float)22), 0), Quaternion.identity);

        byte teamNumber = (byte)(playerCounterTeam0 <= playerCounterTeam1 ? 0 : 1);
        byte botNumber = (byte)(teamNumber == 1 ? 0 : 1);

        if(teamNumber == 0)
        {
            playerCounterTeam0++;
        }
        else
        {
            playerCounterTeam1++;
        }

        p.SetTeamNumber(teamNumber);

        map.PlaceCharacter(p);
        NetworkServer.AddPlayerForConnection(conn, p.gameObject);

        if(!DEBUG_BOTS)
        {
            AddBots(botNumber);
        }
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
        List<Hort> horts = map.NewMap();
        SetHorts(horts);

        if (startGameWithBots)
        {
            bots = new GameObject("Bots");

            bool drawGraph = false;

            // spawn only one bot in debug mode
            if (DEBUG_BOTS)
            {
                //drawGraph = true;
            }

            graph = new Graph(map, drawGraph);
        }

        // get all connections and instanciate a player for each connection
        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn != null)
            {
                CreatePlayer(conn, new CreatePlayerMessage());
            }
        }

        if (DEBUG_BOTS)
        {
            AddBots(1);
        }

        // register connection handler function
        NetworkServer.RegisterHandler<CreatePlayerMessage>(CreatePlayer);

        // handle playtime
        uIManager = GameObject.Find("UIDocument").GetComponent<UIManager>();
        timerIsRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (timerIsRunning)
        {
            if (gameDuration > 0)
            {
                gameDuration -= Time.deltaTime;
                uIManager.SetDuration(gameDuration);
            }
            else
            {
                Debug.Log("Time is finished");
                uIManager.SetDuration(0);
                timerIsRunning = false;
                DetermineWinner(GetHorts());
                // TODO: spiel beenden, display gewinnerteam in ui
            }
        }
        // TODO: if new player joined place player on the map
    }

    private void AddBots(byte teamNumber)
    {
        Bot bot = Instantiate(botPrefab, new Vector3(((float)22) + 0.5f, ((float)22) + 0.5f, 0), Quaternion.identity);
        bot.transform.parent = bots.transform;

        bot.SetTeamNumber(teamNumber);

        if (DEBUG_BOTS)
        {
            Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
            Vector3 botPos = GetRandomTileAroundPlayer((int)playerPos.x, (int)playerPos.y);
            bot.transform.position = botPos;
        }
        else
        {
            map.PlaceCharacter(bot);
        }

        NetworkServer.Spawn(bot.gameObject);

        bot.GetComponent<BotMovement>().SetGraph(graph);
        bot.StartBot();
    }

    // helper function to get a random tile around the player (for debugging)
    public Vector3 GetRandomTileAroundPlayer(int playerX, int playerY)
    {
        Vector3 botPos = new Vector3(-1, -1, 0);
        bool foundTile = false;
        while (!foundTile)
        {
            int x = Random.Range(playerX - 5, playerX + 5);
            int y = Random.Range(playerY - 5, playerY + 5);
            botPos.x = x;
            botPos.y = y;
            if (map.TileIsFree((int)botPos.x, (int)botPos.y) == true && (botPos.x != playerX || botPos.y != playerY))
            {
                foundTile = true;
            }
        }
        return botPos;
    }

    public void DetermineWinner(List<Hort> horts)
    {
        TeamPoints winner = new TeamPoints(0, 0);
        foreach (Hort hort in horts)
        {
            Debug.Log(hort.GetTeamPoints());
            TeamPoints tp = hort.GetTeamPoints();
            if (tp.GetPoints() > winner.GetPoints())
            {
                winner = tp;
            }
        }

        if (winner.GetPoints() == 0)
        {
            Debug.Log("There is no winner!");
        }
        else
        {
            Debug.Log("The winner is team " + winner.GetTeam() + " with " + winner.GetPoints() + " points!");
        }
    }

    public void SetHorts(List<Hort> horts)
    {
        this.horts = horts;
    }

    public List<Hort> GetHorts()
    {
        return this.horts;
    }

    public bool GetDebugBots()
    {
        return this.DEBUG_BOTS;
    }
}
