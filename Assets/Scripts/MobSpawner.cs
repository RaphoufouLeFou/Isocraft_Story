using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    public GameObject mobPrefab;

    public bool spawn;

    public void SpawnMob(Vector3 position)
    {
        Instantiate(mobPrefab, position, Quaternion.identity);
    }

    // Update is called once per frame
    private void Update()
    {
        if (spawn)
        {
            SpawnMob(new Vector3(0,7,0));
            spawn = false;
        }
    }
}
