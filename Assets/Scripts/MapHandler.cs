using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MapHandler : NetworkBehaviour
{
    public GameObject chunkPlane;
    public GameObject chunkParent;
    public Material material;

    private Transform _chunksParent;
    [NonSerialized] public static Dictionary<string, Chunk> Chunks;
    
    /*
    [Command(requiresAuthority = false)]
    public void RequestChunks()
    {
        Debug.Log($"Spawning chunks");
        foreach (var chunk in Chunks)
        {

            NetworkManager nm = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            nm.spawnPrefabs.Add(chunk.Value.gameObject);

            System.Guid creatureAssetId = System.Guid.NewGuid();
            
            NetworkClient.RegisterPrefab(chunk.Value.gameObject, 69);
           

            Debug.Log($"Spawning chunk ({chunk.Key})");
            NetworkServer.Spawn(chunk.Value.gameObject);
        }

    }*/

    void Start()
    {
        NetworkManagement manager = GameObject.Find("NetworkManager").GetComponent<NetworkManagement>();
        Transform parent = chunkParent.transform;

        // if (NetworkInfos.IsHost) {
        _chunksParent = chunkParent.transform;
        Chunks = new Dictionary<string, Chunk>();
        transform.position = new Vector3(0, 0, 0);

        for (int x = -4; x < 5; x++)
        for (int z = -4; z < 5; z++)
            GenChunk(x, z);
        
        //else
        //{
            //while (!manager.IsChunksReady) ;

        //    Debug.Log("Requesting chunks");
        //    RequestChunks();
        //    Debug.Log("Requested chunks");
            /*
            foreach (var chunk in Chunks)
            {
                GameObject InstanciedChunk = Instantiate(chunk.Value.gameObject, parent);
                InstanciedChunk.name = chunk.Key;
            }*/
        //}
    }
    
    void GenChunk(int x, int z)
    {
        Vector3 pos = new Vector3(x, 0, z);
        GameObject chunkObject = Instantiate(chunkPlane, _chunksParent);
        chunkObject.name = pos.x + "." + pos.z;

        Chunk chunk = chunkObject.GetComponent<Chunk>();
        MeshRenderer meshRenderer = chunkObject.GetComponent<MeshRenderer>();
        meshRenderer.material = material;
        Chunks.Add(chunkObject.name, chunk);
        chunk.Init(pos);
    }
}
