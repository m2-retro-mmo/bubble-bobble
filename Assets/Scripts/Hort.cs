using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class Hort : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnDiamondsChanged))]
    public int diamonds = 0;
    [SyncVar]
    public byte team = 1;
    public static byte scale = 1;

    [Header("UI Text")]

    [SerializeField]
    private TextMeshProUGUI teamPoints_text;

    public GameObject plusOnePrefab;
    private Vector3 plusOneSpawningOffset = new Vector3(0, 3, 0);

    public void Awake()
    {
        gameObject.transform.localScale = new Vector3(scale, scale, 0);
    }

    public void init(byte teamNumber)
    {
        team = teamNumber;
        teamPoints_text = GameObject.Find("PointsTeam" + team.ToString() + "Value_Text").GetComponent<TextMeshProUGUI>();
    }

    [Server]
    public void AddDiamond()
    {
        diamonds++;
        SpawnPlusOne();
        teamPoints_text.text = diamonds.ToString();
    }

    private void OnDiamondsChanged(int oldDiamonds, int newDiamonds)
    {
        teamPoints_text.text = diamonds.ToString();
    }

    [Server]
    private void SpawnPlusOne()
    {
        GameObject plusOne = Instantiate(plusOnePrefab, gameObject.transform.position + plusOneSpawningOffset, gameObject.transform.rotation);
        NetworkServer.Spawn(plusOne);
    }

    public byte GetTeamNumber()
    {
        return team;
    }
}
