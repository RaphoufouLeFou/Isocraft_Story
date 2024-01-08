using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MapHandler : NetworkBehaviour
{
    public GameObject chunkPlane;
    public GameObject chunkParent;
    public Material material;

    [NonSerialized] public static Dictionary<string, Chunk> Chunks;


    [Command(requiresAuthority = false)]
    public void RequestChunks()
    {
        Debug.LogError($"Spawning chunks");
        foreach (var chunk in Chunks)
        {

            NetworkManager Nm = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            Nm.spawnPrefabs.Add(chunk.Value.gameObject);

            System.Guid creatureAssetId = System.Guid.NewGuid();
            
            NetworkClient.RegisterPrefab(chunk.Value.gameObject, 69);
           

            Debug.LogError($"Spawning chunk ({chunk.Key})");
            NetworkServer.Spawn(chunk.Value.gameObject);
        }

    }

    void Start()
    {
        NetworkManagement manager = GameObject.Find("NetworkManager").GetComponent<NetworkManagement>();
        Transform parent = chunkParent.transform;

        if (/*NetworkInfos.IsHost*/ 1==1) {
            Chunks = new Dictionary<string, Chunk>();
            transform.position = new Vector3(0, 0, 0);
            

            for (int x = -4; x < 5; x++)
                for (int z = -4; z < 5; z++)
                {
                    Vector3 pos = new Vector3(x, 0, z);
                    GameObject chunkObject = Instantiate(chunkPlane, parent);
                    
                    chunkObject.name = pos.x + "." + pos.z;

                    NetworkIdentity Ni = chunkObject.GetComponent<NetworkIdentity>();

                    Debug.LogWarning($"{ Ni.assetId}");
                    Chunk chunk = chunkObject.GetComponent<Chunk>();
                    MeshRenderer meshRenderer = chunkObject.GetComponent<MeshRenderer>();
                    meshRenderer.material = material;
                    chunk.Init(pos);



                    
                    

                    Chunks.Add(chunkObject.name, chunk);
                } 
            manager.ReadyToSendChunks(Chunks);
        }
        else
        {
            //while (!manager.IsChunksReady) ;

            Debug.LogError("Requesting chunks");
            RequestChunks();
            Debug.LogError("Requested chunks");
            /*
            foreach (var chunk in Chunks)
            {
                GameObject InstanciedChunk = Instantiate(chunk.Value.gameObject, parent);
                InstanciedChunk.name = chunk.Key;
            }*/
        }
    }
}
