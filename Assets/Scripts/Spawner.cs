using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    // Start is called before the first frame update

    public float minSpawnDistance;
    private float lastEnemySpawn = 0;
    public float sizeMap;

    public GameObject being;

    public List<Species> species;

    private void Start()
    {
        // foreach (Species s in species)
        // {
        //     s.lastSpawn = 0;

        // }
    }
    // Update is called once per frame
    void Update()
    {

        // foreach (Species s in species)
        // {
        //     Debug.Log($"Spawn {s.name}");
        //     Debug.Log(Time.time - s.lastSpawn);
        //     Debug.Log(s.spawnRate);

        //     if (Time.time - s.lastSpawn > 1 / s.spawnRate)
        //     {
        //         s.lastSpawn = Time.time;
        //         Vector3 spawnLocation = new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 1);

        //         if (s.lastBeingSpawned != null)
        //         {
        //             spawnLocation = s.lastBeingSpawned.transform.position;
        //         }

        //         GameObject g = Instantiate(being, spawnLocation, Quaternion.identity);
        //         g.GetComponent<Being>().Birth(s);

        //     }
        // }

    }

}
