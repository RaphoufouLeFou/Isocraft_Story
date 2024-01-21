using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Mirror;

public class MapHandler : NetworkBehaviour
{
    public GameObject chunkPlane;
    public GameObject chunkParent;
    public Material material;

    private Transform _chunksParent;
    [NonSerialized] public static Dictionary<string, Chunk> Chunks;

    void Start()
    {
        //NetworkManagement manager = GameObject.Find("NetworkManager").GetComponent<NetworkManagement>();
        //Transform parent = chunkParent.transform;

        _chunksParent = chunkParent.transform;
        Chunks = new Dictionary<string, Chunk>();
        transform.position = new Vector3(0, 0, 0);
        
        for (int x = -4; x < 5; x++)
        for (int z = -4; z < 5; z++)
            GenChunk(x, z);
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
        int res = LoadChunksMesh(chunk);
        chunk.Init(pos, res == 0);
        SaveChunks(chunk);
    }
    public void SaveChunks(Chunk chunk)
    {
        if(SaveInfos.SaveName == "") return;
        string dirPath = Application.persistentDataPath + "/Saves/" + SaveInfos.SaveName + "/Chunks/";
        string path = dirPath + chunk.name + ".Chunk";
        int[,,] blocks = chunk.Blocks;
        int size = chunk.GetSize();
        if(File.Exists(path)) File.Delete(path);
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        FileStream fs = new FileStream(path, FileMode.Create);
        
        for (int i = 0; i < size; i++)
        for (int j = 0; j < size; j++)
        for (int k = 0; k < size; k++)
        {
            int currBlock = blocks[i, j, k];
            
            // saving as 16bit number for now, we don't need more than 32 000 blocks types
            
            //fs.WriteByte((byte)((currBlock >> 24) & 0xFF));
            //fs.WriteByte((byte)((currBlock >> 16) & 0xFF));
            fs.WriteByte((byte)((currBlock >> 8) & 0xFF));
            fs.WriteByte((byte)(currBlock & 0xFF));
        }
        fs.Close();
    }
    
    
    public int LoadChunksMesh(Chunk chunk)
    {
        if(SaveInfos.SaveName == "") return 1;
        string path = Application.persistentDataPath + "/Saves/" + SaveInfos.SaveName + "/Chunks/" + chunk.name + ".Chunk";
        if (!File.Exists(path)) return 1;
        
        int size = chunk.GetSize();
        
        int[,,] blocks = chunk.Blocks;
        
        FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
        
        for (int i = 0; i < size; i++)
        for (int j = 0; j < size; j++)
        for (int k = 0; k < size; k++)
        {
            int currBlock;
            
            //currBlock = fs.ReadByte() << 24;
            //currBlock |= fs.ReadByte() << 16;
            currBlock = fs.ReadByte() << 8;
            currBlock |= fs.ReadByte();
            blocks[i, j, k] = currBlock;
        }
        fs.Close();
        return 0;
    }
    
}


