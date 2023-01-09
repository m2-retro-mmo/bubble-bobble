using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UIElements;

public class Hort : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnDiamondsChanged))]
    public int diamonds = 0;
    [SyncVar]
    public byte team = 1;
    public static byte scale = 7;

    private UIManager uIManager;

    [Header("UI Text")]

    public GameObject plusOnePrefab;


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
        // TODO: add to parent GameObject to get better structure in scecne instance
        GameObject plusOne = Instantiate(plusOnePrefab, gameObject.transform.position, gameObject.transform.rotation);
        NetworkServer.Spawn(plusOne);
    }

    public byte GetTeamNumber()
    {
        return team;
    }
}
