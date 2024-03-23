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

        if (isServer)
        {
            Game.SaveManager.IsHost = true;
            if (SuperGlobals.IsNewSave) Game.SaveManager.SaveGame(); // initial save
            else Game.SaveManager.LoadSave();
        }
        else CmdRequestGameName(NetworkClient.localPlayer.GetInstanceID());
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
            
            chunk.Init(x, z, !LoadBlocks(chunk.Blocks, chunk.name), true);
            SaveChunk(chunk);
        }
        else
        {
            int id = NetworkClient.localPlayer.GetInstanceID();
            CmdRequestBlocks(x, z, id);
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

    private static bool LoadBlocks(int[,,] blocks, string chunkName)
    {
        // returns false if couldn't find file, meaning the blocks have to be generated next

        if (!SuperGlobals.StartedFromMainMenu) return false;
        
        string path = $"{Application.persistentDataPath}/Saves/{Game.SaveManager.SaveName}/Chunks/{chunkName}.Chunk";
        if (!File.Exists(path)) return false;
        
        FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
        
        for (int i = 0; i < Chunk.Size; i++) for (int j = 0; j < Chunk.Size; j++)
        for (int k = 0; k < Chunk.Size; k++)
        {
            int currBlock = 0;
            
            currBlock |= fs.ReadByte();
            blocks[i, j, k] = currBlock;
        }
        
        fs.Close();
        return true;
    }

    [Command (requiresAuthority = false)]
    private void CmdRequestGameName(int id)
    {
        RpcSendGameName(Game.SaveManager.SaveName, id);
    }

    [ClientRpc]
    private void RpcSendGameName(string gameName, int id)
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
    private void CmdRequestBlocks(int x, int z, int id)
    {
        // prepare a chunk, or load it, to send its blocks to a client

        int[,,] blocks = new int[Chunk.Size, Chunk.Size, Chunk.Size];
        string chunkName = x + "." + z;
        Chunk chunk;
        if (Chunks.TryGetValue(chunkName, out chunk)) blocks = chunk.Blocks;
        else
        {
            if (!LoadBlocks(blocks, chunkName)) // chunk doesn't exist yet
            {
                GameObject chunkObject = Instantiate(chunkPlane, _chunksParent);
                chunk = chunkObject.GetComponent<Chunk>();
                chunk.GenerateBlocks();
                
                blocks = chunk.Blocks;
            }
        }
        
        int len = blocks.Length;
        int[] bytes = new int[len];
        Buffer.BlockCopy(blocks, 0, bytes, 0, len * 4);
        
        RpcSendBlocks(bytes, x, z, id);
    }
    
    [ClientRpc]
    private void RpcSendBlocks(int[] bytes, int x, int z, int id)
    {
        // this is where a chunk is generated on the client

        if (NetworkClient.localPlayer.GetInstanceID() != id) return; // only send to a specific client
        
        int[,,] blocks = new int[Chunk.Size,Chunk.Size,Chunk.Size];
        Buffer.BlockCopy(bytes, 0, blocks, 0, bytes.Length * 4);
        
        GameObject chunkObject = Instantiate(chunkPlane, _chunksParent);
        chunkObject.name = x + "." + z;
        
        Chunk chunk = chunkObject.GetComponent<Chunk>();
        MeshRenderer meshRenderer = chunkObject.GetComponent<MeshRenderer>();
        meshRenderer.material = material;
        
        chunk.Blocks = blocks;

        Chunks.TryAdd(chunkObject.name, chunk);
        chunk.Init(x, z, true, true);
    }
}