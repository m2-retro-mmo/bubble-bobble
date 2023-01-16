using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameGenerator : MonoBehaviour
{
    private static string[] animals;
    private static string[] adjectives;

    private string animalsPath = "Assets/Resources/NameGenerator/animals.txt";
    private string adjectivesPath = "Assets/Resources/NameGenerator/adjectives.txt";

    private void Start()
    {
        // read the file
        animals = System.IO.File.ReadAllLines(animalsPath);
        adjectives = System.IO.File.ReadAllLines(adjectivesPath);

        for (int i = 0; i < 10; i++)
        {
            Debug.Log(GetRandomName());
        }
    }

    public static string GetRandomName()
    {
        System.Random random = new System.Random();
        
        int rand = random.Next(0, adjectives.Length - 1);
        string adj = adjectives[rand].ToLower();
        
        rand = random.Next(0, animals.Length - 1);
        string name = animals[rand];

        string generatedName = adj + name;

        return generatedName;
    }
}
