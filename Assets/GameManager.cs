using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    public GameObject botPrefab;
    public Map map;
    public Camera cam;

    private byte teamCount = 2;

    // Start is called before the first frame update
    void Start()
    {
        // instanciate a Hort for each Team
        for (byte teamNumber = 0; teamNumber < teamCount; teamNumber++)
        {
            Hort hort = Instantiate(hortPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            hort.team = teamNumber;
            map.PlaceHort(hort);
        }

        // instanciate a local player
        Player player = Instantiate(playerPrefab, new Vector3(((float)22) + 0.5f, ((float)22) + 0.5f, 0), Quaternion.identity);
        player.cam = cam;
        player.setTeamNumber(1);
        map.PlacePlayer(player);

        // camera should follow main player
        CameraFollow camFollow = cam.GetComponent<CameraFollow>();
        camFollow.target = player.transform;
        // TODO: place some bots random on the map
        // GameObject bot = Instantiate(botPrefab, new Vector3(Random.Range(-39, 10), Random.Range(-4, 23), 0), Quaternion.identity);
        // BotMovement botMovement = bot.GetComponent<BotMovement>() as BotMovement;

    }

    // Update is called once per frame
    void Update()
    {
        // TODO: if new player joined place player on the map
    }
}
