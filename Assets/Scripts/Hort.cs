using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Hort : MonoBehaviour
{
    public int diamonds = 0;
    public byte team = 0;

    // public TextMeshProUGUI hortPointCounter;

    public GameObject plusOnePrefab;

    public void AddDiamond()
    {
        diamonds++;
        SpawnPlusOne();
        // hortPointCounter.text = diamonds.ToString();
    }

    private void SpawnPlusOne()
    {
        GameObject plusOne = Instantiate(plusOnePrefab, gameObject.transform.position, gameObject.transform.rotation);
    }
}
