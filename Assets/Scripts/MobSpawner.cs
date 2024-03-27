using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    public GameObject mobPrefab;

    public bool spawn;

    public void SpawnMob(Vector3 position, int Mob)
    {
        GameObject mob = Instantiate(mobPrefab, position, Quaternion.identity);
        MobAI ai = mob.GetComponent<MobAI>();
        ai.SetName(Mob);
    }

    // Update is called once per frame
    private void Update()
    {
        if (spawn)
        {
            SpawnMob(new Vector3(0,7,0), 0);
            spawn = false;
        }
    }
}
