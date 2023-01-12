using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UIElements;

public class TeamPoints {
    private int team;
    private int points;

    public TeamPoints(int team, int points)
    {
        this.team = team;
        this.points = points;
    }

    public int GetTeam()
    {
        return team;
    }

    public int GetPoints()
    {
        return points;
    }
}

public class Hort : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnDiamondsChanged))]
    public int diamonds = 0;
    [SyncVar]
    public byte team = 1;
    public static byte scale = 1;

    private UIManager uIManager;

    [Header("UI Text")]

    public GameObject plusOnePrefab;
    private Vector3 plusOneSpawningOffset = new Vector3(0, 3, 0);

    public void Awake()
    {
        gameObject.transform.localScale = new Vector3(scale, scale, 0);
    }

    public void init(byte teamNumber)
    {
        team = teamNumber;
        uIManager = GameObject.Find("UIDocument").GetComponent<UIManager>();
    }

    [Server]
    public void AddDiamond()
    {
        diamonds++;
        SpawnPlusOne();
    }

    private void OnDiamondsChanged(int oldDiamonds, int newDiamonds)
    {
        uIManager.AddTeamPoint(team);
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

    public int GetPoints()
    {
        return diamonds;
    }

    public TeamPoints GetTeamPoints()
    {
        TeamPoints tp = new TeamPoints(GetTeamNumber(), GetPoints());
        return tp;
    }
}
