using Unity.VisualScripting;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    public bool spawn;

    public void SpawnMob(Vector3 position, int mobID)
    {
        GameObject mob = Instantiate(Game.MobPrefabs[mobID], position, Quaternion.identity);
        IAiControlled ai = mob.GetComponent<IAiControlled>();
        ai.Init(mobID);
    }

    // Update is called once per frame
    private void Update()
    {
        if (spawn)
        {
            SpawnMob(new Vector3(0, 7, 0), 2);
            spawn = false;
        }
    }
}
