using System;
using System.Collections.Generic;
using UnityEngine;

public class MapHandler : MonoBehaviour
{
    public GameObject chunkPlane;
    public GameObject chunkParent;
    public Material material;

    [NonSerialized] public static Dictionary<string, Chunk> Chunks;

    void Start()
    {
        Chunks = new Dictionary<string, Chunk>();
        transform.position = new Vector3(0, 0, 0);
        Transform parent = chunkParent.transform;

        for (int x = -4; x < 5; x++)
            for (int z = -4; z < 5; z++)
            {
                Vector3 pos = new Vector3(x, 0, z);
                GameObject chunkObject = Instantiate(chunkPlane, parent);
                chunkObject.name = pos.x + "." + pos.z;

                Chunk chunk = chunkObject.GetComponent<Chunk>();
                MeshRenderer meshRenderer = chunkObject.GetComponent<MeshRenderer>();
                meshRenderer.material = material;
                chunk.Init(pos);
                Chunks.Add(chunkObject.name, chunk);
            }
    }
}
