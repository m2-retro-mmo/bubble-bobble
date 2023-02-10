using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameGenerator : MonoBehaviour
{
    private static string[] animals;
    private static string[] adjectives;

    private void Start()
    {
        // read the file
        LoadData();
    }

    public static string GetRandomName()
    {
        System.Random random = new System.Random();
        if (animals == null || adjectives == null)
        {
            LoadData();
        }

        int rand = random.Next(0, adjectives.Length - 1);
        string adj = adjectives[rand].ToLower();

        rand = random.Next(0, animals.Length - 1);
        string name = animals[rand];

        string generatedName = adj + name;

        return generatedName;
    }

    public static void LoadData()
    {
        // Load asset from unity assetDatabase
        var animalsFile = Resources.Load<TextAsset>("NameGenerator/animals").text;
        var adjectivesFile = Resources.Load<TextAsset>("NameGenerator/adjectives").text;

        animals = animalsFile.Split("\n", System.StringSplitOptions.RemoveEmptyEntries);
        adjectives = adjectivesFile.Split("\n", System.StringSplitOptions.RemoveEmptyEntries);
    }
}
