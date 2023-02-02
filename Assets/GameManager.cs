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
    [Tooltip("true if the bot should be spawned in the area of the player")]
    private bool DEBUG_BOTS = false;

    [SerializeField]
    [Tooltip("The prefab of the bot")]
    private Bot botPrefab;

    [SerializeField]
    [Tooltip("The number of Characters (Bots and Player) the game should start with")]
    private int characterCount;

    [SyncVar (hook = nameof(DurationUpdated))] private float gameDuration = 100f;
    private bool timerIsRunning;

    private int botTeamNumber;
    private byte winnerForTracking;

    private List<Hort> horts;

    public static GameManager singleton { get; internal set; }

    bool InitializeSingleton()
    {
        if (singleton != null && singleton == this)
            return true;

        if (singleton != null)
        {
            Debug.LogError("Multiple GameManagers in the scene");
            return false;
        }

        singleton = this;
        return true;
    }

    void OnDestroy()
    {
        if (singleton == this)
            singleton = null;
    }

    private GameObject bots;

    private Graph graph;

    private byte botCounterTeam0 = 0;
    private byte botCounterTeam1 = 0;

    private byte playerCounterTeam0 = 0;
    private byte playerCounterTeam1 = 0;

    private void Start() {
        uIManager = GameObject.Find("UIDocument").GetComponent<UIManager>();
    }

    // Start is called before the first frame update
    [Server]
    public override void OnStartServer()
    {
        InitializeSingleton();
        if (isServerOnly)
        {
            cam = GameObject.Find("Server Camera").GetComponent<Camera>();
            cam.enabled = true;
            cam.GetComponent<Camera>().enabled = true;
            cam.GetComponent<AudioListener>().enabled = true;
        }
        List<Hort> horts = map.NewMap();
        SetHorts(horts);

        gameDuration = (BBNetworkManager.singleton as BBNetworkManager).gameDuration;

        bots = new GameObject("Bots");
        bool drawGraph = false;
        // spawn only one bot in debug mode
        if (DEBUG_BOTS)
        {
            //drawGraph = true;
        }
        graph = new Graph(map, drawGraph);

        if (!DEBUG_BOTS)
            CreateBots();

        // get all connections and instanciate a player for each connection
        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn != null)
            {
                CreatePlayer(conn);
            }
        }

        // handle playtime
        timerIsRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isClientOnly) return;
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

    private void DurationUpdated(float oldDuration, float newDuration)
    {
        if (uIManager != null)
            uIManager.SetDuration(gameDuration);
    }

    public void CreatePlayer(NetworkConnectionToClient conn)
    {
        foreach (Player player in FindObjectsOfType<Player>())
        {
            if (player.connectionToClient == conn)
            {
                Debug.Log("Player already exists");
                return;
            }
        }
        Debug.Log("Create player for connection: " + conn.connectionId);

        Player p = Instantiate(playerPrefab, new Vector3(((float)22), ((float)22), 0), Quaternion.identity);

        p.playerName = (BBNetworkManager.singleton as BBNetworkManager).getPlayerName(conn);

        byte teamNumber = (byte)(playerCounterTeam0 <= playerCounterTeam1 ? 0 : 1);

        if (teamNumber == 0)
        {
            playerCounterTeam0++;
        }
        else
        {
            playerCounterTeam1++;
        }

        p.SetTeamNumber(teamNumber);

        map.PlaceCharacter(p);

        // check if there is already a player for the connection
        if (conn.identity != null && conn.identity.gameObject != null)
        {
            // NetworkManager.Destroy(conn.identity.gameObject);
            NetworkServer.ReplacePlayerForConnection(conn.identity.connectionToClient, p.gameObject, true);
        }
        else
        {
            NetworkServer.AddPlayerForConnection(conn, p.gameObject);
        }

        if (!DEBUG_BOTS)
        {
            RemoveBot(teamNumber);
        }
        else
        {
            byte botTeamNumber = (byte)(teamNumber == 0 ? 1 : 0);
            AddBot(botTeamNumber);
        }
    }

    public void RemovePlayer(NetworkConnectionToClient conn)
    {
        Player p = conn.identity.GetComponent<Player>();
        byte teamNumber = p.GetTeamNumber();
        AddBot(teamNumber);
    }

    private void CreateBots()
    {
        // check if character count is even
        if (characterCount % 2 != 0)
        {
            characterCount--;
        }
        // check if character count is greater than 100
        if (characterCount > 100)
        {
            characterCount = 100;
        }
        for (int i = 1; i <= characterCount; i++)
        {
            byte teamNumber = (byte)(i % 2 == 0 ? 0 : 1);
            if (teamNumber == 0)
            {
                botCounterTeam0++;
            }
            else
            {
                botCounterTeam1++;
            }
            AddBot(teamNumber);
        }
    }

    private void AddBot(byte teamNumber)
    {
        Bot bot = Instantiate(botPrefab, new Vector3(((float)22) + 0.5f, ((float)22) + 0.5f, 0), Quaternion.identity);
        bot.transform.parent = bots.transform;

        bot.SetTeamNumber(teamNumber);
        bot.RandomizeInteractionWeights();

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

    private void RemoveBot(byte teamNumber)
    {
        if (teamNumber == 0)
        {
            botCounterTeam0--;
        }
        else
        {
            botCounterTeam1--;
        }
        // find bot with team number
        foreach (Bot bot in FindObjectsOfType<Bot>())
        {
            if (bot.GetTeamNumber() == teamNumber)
            {
                NetworkManager.Destroy(bot.gameObject);
                return;
            }
        }
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
            winnerForTracking = (byte)winner.GetTeam();
            Debug.Log("The winner is team " + winner.GetTeam() + " with " + winner.GetPoints() + " points!");
        }
        (BBNetworkManager.singleton as BBNetworkManager).returnToLobby();
    }

    public byte GetWinnerForTracking()
    {
        return winnerForTracking;
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
