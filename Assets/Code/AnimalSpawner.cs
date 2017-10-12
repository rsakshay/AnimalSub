using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour {

    public GameObject animalPrefab;
    public int MAX_ANIMALS = 5;

    List<GameObject> animalList = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < MAX_ANIMALS; i++)
        {
            SpawnAnimal();
        }
    }

    void SpawnAnimal()
    {
        Vector2 randScreenPoint = new Vector2(Random.Range(0.2f, 0.8f), Random.Range(0.2f, 0.8f));
        Vector2 spawnPoint = Camera.main.ViewportToWorldPoint(randScreenPoint);

        GameObject animal = Instantiate(animalPrefab, spawnPoint, Quaternion.identity);
        animalList.Add(animal);
    }
}
