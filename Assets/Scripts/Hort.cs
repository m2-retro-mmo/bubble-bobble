using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class Hort : NetworkBehaviour
{
    public int diamonds = 0;
    public byte team = 1;
    public byte width = 7;
    public byte height = 7;

    [Header("UI Text")]

    [SerializeField]
    private TextMeshProUGUI teamPoints_text;

    public GameObject plusOnePrefab;

    public void init(byte teamNumber)
    {
        team = teamNumber;
        gameObject.transform.localScale = new Vector3(width, height, 0);
        teamPoints_text = GameObject.Find("PointsTeam" + team.ToString() + "Value_Text").GetComponent<TextMeshProUGUI>();
    }

    public void AddDiamond()
    {
        diamonds++;
        SpawnPlusOne();
        teamPoints_text.text = diamonds.ToString();
    }

    private void SpawnPlusOne()
    {
        GameObject plusOne = Instantiate(plusOnePrefab, gameObject.transform.position, gameObject.transform.rotation);
    }
}
