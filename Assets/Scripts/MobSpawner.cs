using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{

    public GameObject mobPrefab;

    public bool spawn;

    public void SpawnMob(Vector3 position)
    {
        Instantiate(mobPrefab, position, Quaternion.identity);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (spawn)
        {
            SpawnMob(new Vector3(0,7,0));
            spawn = false;
        } 
    }
}
