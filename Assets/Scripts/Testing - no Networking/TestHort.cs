using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestHort : MonoBehaviour
{
    public int diamonds = 0;
    public byte team = 1;
    public static byte scale = 1;

    [Header("UI Text")]

    [SerializeField]
    private TextMeshProUGUI teamPoints_text;

    public GameObject plusOnePrefab;
    private Vector3 plusOneSpawningOffset = new Vector3(0, 3, 0);

    public void Awake()
    {
        teamPoints_text = GameObject.Find("PointsTeam" + team.ToString() + "Value_Text").GetComponent<TextMeshProUGUI>();
        gameObject.transform.localScale = new Vector3(scale, scale, 0);
    }

    public void init(byte teamNumber)
    {
        team = teamNumber;
    }

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

    private void SpawnPlusOne()
    {
        GameObject plusOne = Instantiate(plusOnePrefab, gameObject.transform.position + plusOneSpawningOffset, gameObject.transform.rotation);
    }

    public byte GetTeamNumber()
    {
        return team;
    }
}