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
    
    public void StartMapHandle()
    {
        _chunksParent = chunkParent.transform;
        Chunks = new Dictionary<string, Chunk>();
        transform.position = new Vector3(0, 0, 0);
        
        for (int x = -4; x < 5; x++)
            for (int z = -4; z < 5; z++)
                GenChunk(x, z);
        
        if (!isServer) RequestGameName(NetworkClient.localPlayer.GetInstanceID());
        else
        {
            Game.SaveManager.IsHost = true;
            if (SuperGlobals.IsNewSave) Game.SaveManager.SaveGame(); // initial save
            else Game.SaveManager.LoadSave();
        }
    }

    private void GenChunk(int x, int z)
    {
        if (isServer)
        {
            GameObject chunkObject = Instantiate(chunkPlane, _chunksParent);
            chunkObject.name = x + "." + z;
        
            Chunk chunk = chunkObject.GetComponent<Chunk>();
            MeshRenderer meshRenderer = chunkObject.GetComponent<MeshRenderer>();
            meshRenderer.material = material;
            Chunks.TryAdd(chunkObject.name, chunk);
            
            int res = LoadChunksMesh(chunk);
            chunk.Init(x, z, res == 0);
            SaveChunk(chunk);
        }
        else
        {
            int id = NetworkClient.localPlayer.GetInstanceID();
            RequestChunk(x, z, id);
        }
    }

    public static void SaveAllChunks()
    {
        foreach (Chunk chunk in Chunks.Values) SaveChunk(chunk);
    }
    
    public static void SaveChunk(Chunk chunk)
    {
        if (!SuperGlobals.StartedFromMainMenu) return;
        
        string dirPath = $"{Application.persistentDataPath}/Saves/{Game.SaveManager.SaveName}/Chunks/";
        string path = $"{dirPath}{chunk.name}.Chunk";
        int[,,] blocks = chunk.Blocks;
        int size = Chunk.Size;
        if (File.Exists(path)) File.Delete(path);
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        FileStream fs = new FileStream(path, FileMode.Create);
        
        for (int i = 0; i < size; i++) for (int j = 0; j < size; j++)
        for (int k = 0; k < size; k++)
            fs.WriteByte((byte)(blocks[i, j, k] & 0xFF));
        fs.Close();
    }
    
    private static int LoadChunksMesh(Chunk chunk)
    {
        if (!SuperGlobals.StartedFromMainMenu) return 1;
        
        string path = Application.persistentDataPath + "/Saves/" + Game.SaveManager.SaveName + "/Chunks/" + chunk.name + ".Chunk";
        
        if (!File.Exists(path)) return 1;
        
        int size = Chunk.Size;
        
        int[,,] blocks = chunk.Blocks;
        
        FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
        
        for (int i = 0; i < size; i++)
        for (int j = 0; j < size; j++)
        for (int k = 0; k < size; k++)
        {
            int currBlock = 0;
            
            currBlock |= fs.ReadByte();
            blocks[i, j, k] = currBlock;
        }
        
        fs.Close();
        return 0;
    }

    [Command(requiresAuthority = false)]
    private void RequestGameName(int id)
    {
        SendGameName(Game.SaveManager.SaveName, id);
    }

    [ClientRpc]
    private void SendGameName(string gameName, int id)
    {
        if (NetworkClient.localPlayer.GetInstanceID() != id) return;
        SuperGlobals.SaveName = gameName;
        Game.SaveManager.SaveName = gameName;
        Game.SaveManager.IsHost = false;
        string clientName = "CLIENT__" + gameName;
        string path = Application.persistentDataPath + $"/Saves/{clientName}/{clientName}.IsoSave";
        if (!File.Exists(path)) Game.SaveManager.SaveGame(); // initial save
        else Game.SaveManager.LoadSave();
    }
    
    [Command (requiresAuthority = false)]
    private void RequestChunk(int x, int z, int id) // running on the server
    {
        int[,,] blocks = GameObject.Find(x + "." + z).GetComponent<Chunk>().Blocks;
        int len = blocks.Length;
        int[] bytes = new int[len];
        Buffer.BlockCopy(blocks, 0, bytes, 0, len * 4);
        
        SendChunk(bytes, x, z, id);
    }
    
    [ClientRpc]
    private void SendChunk(int[] bytes, int x, int z, int id) // running on the client
    {
        if (NetworkClient.localPlayer.GetInstanceID() != id) return;
        
        int[,,] blocks = new int[Chunk.Size,Chunk.Size,Chunk.Size];
        Buffer.BlockCopy(bytes, 0, blocks, 0, bytes.Length*4);
        
        GameObject chunkObject = Instantiate(chunkPlane, _chunksParent);
        chunkObject.name = x + "." + z;
        
        Chunk chunk = chunkObject.GetComponent<Chunk>();
        MeshRenderer meshRenderer = chunkObject.GetComponent<MeshRenderer>();
        meshRenderer.material = material;
        
        chunk.Blocks = blocks;

        Chunks.TryAdd(chunkObject.name, chunk);
        chunk.Init(x, z, true);
    }
}