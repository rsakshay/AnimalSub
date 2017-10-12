using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour {

    public GameObject foodPrefab;
    public int MAX_FOOD = 5;

    List<GameObject> foodList = new List<GameObject>();

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        foodList.Clear();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Food"))
            foodList.Add(go);

        if (foodList.Count < MAX_FOOD)
            SpawnFood();
	}

    void SpawnFood()
    {
        Vector2 randScreenPoint = new Vector2(Random.Range(0.2f, 0.8f), Random.Range(0.2f, 0.8f));
        Vector2 spawnPoint = Camera.main.ViewportToWorldPoint(randScreenPoint);

        GameObject food = Instantiate(foodPrefab, spawnPoint, Quaternion.identity);
        foodList.Add(food);
    }
}
