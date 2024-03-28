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
                ai = mob.AddComponent<MobBopako>();
                break;
            case 2:
                ai = mob.AddComponent<MobPokabo>();
                break; 
            case 3:
                ai = mob.AddComponent<MobOakBoka>();
                break; 
            case 4:
                ai = mob.AddComponent<MobChefBoka>();
                break; 
            default: 
                ai = mob.AddComponent<MobPokabo>();
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
            for (int i = 1; i < 5; i++)
            {
                SpawnMob(new Vector3(0, 7, 0), i);
            }
        }
    }
}
