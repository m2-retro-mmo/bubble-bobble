using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Hort : MonoBehaviour
{
    public int diamonds = 0;
    public byte team = 1;

    [Header("UI Text")]

    [SerializeField]
    private TextMeshProUGUI teamPoints_text;

    // public TextMeshProUGUI hortPointCounter;

    public GameObject plusOnePrefab;

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
