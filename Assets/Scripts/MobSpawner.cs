using Unity.VisualScripting;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    public bool spawn;

    public void SpawnMob(Vector3 position, int mobID)
    {
        GameObject mob = Instantiate(Game.MobPrefabs[mobID], position, Quaternion.identity);
        IAiControlled ai;
        switch (mobID)
        {
            case 1:
                ai = mob.AddComponent<MobClassic>();
                break;
            case 2:
                ai = mob.AddComponent<MobZapatos>();
                break; 
            default: 
                ai = mob.AddComponent<MobZapatos>();
                break;
        }
        
        ai.Init(mobID);
    }

    // Update is called once per frame
    private void Update()
    {
        if (spawn)
        {
            spawn = false;
            SpawnMob(new Vector3(0, 7, 0), 2);
        }
    }
}
