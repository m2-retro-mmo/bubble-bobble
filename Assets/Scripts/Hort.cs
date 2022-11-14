using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Hort : MonoBehaviour
{
    public int diamonds = 0;
    public byte team = 0;

    public TextMeshProUGUI hortPointCounter;

    public void AddDiamond()
    {
        diamonds++;
        hortPointCounter.text = diamonds.ToString();
    }
}
