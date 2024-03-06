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
        if (!isServer)
            RequestGameName(NetworkClient.localPlayer.GetInstanceID());

        for (int x = -4; x < 5; x++)
        for (int z = -4; z < 5; z++)
        {
            GenChunk(x, z, x == 4 && z == 4);
        }

    
    }

    private void GenChunk(int x, int z, bool last)
    {
        if (!isServer)
        {
            int id = NetworkClient.localPlayer.GetInstanceID();
            RequestChunk(x, z, id, last);
        }
        else
        {

            Vector3 pos = new Vector3(x, 0, z);
            GameObject chunkObject = Instantiate(chunkPlane, _chunksParent);
            chunkObject.name = pos.x + "." + pos.z;
        
            Chunk chunk = chunkObject.GetComponent<Chunk>();
            MeshRenderer meshRenderer = chunkObject.GetComponent<MeshRenderer>();
            meshRenderer.material = material;
            Chunks.TryAdd(chunkObject.name, chunk);
            
            int res = LoadChunksMesh(chunk);
            chunk.Init(pos, res == 0);
            SaveChunks(chunk);
        }
    }
    
    public void SaveChunks(Chunk chunk)
    {
        if (!SuperGlobals.StartedFromMainMenu) return;

        string dirPath = Application.persistentDataPath + "/Saves/" + Game.SaveManager.SaveName + "/Chunks/";
        string path = dirPath + chunk.name + ".Chunk";
        int[,,] blocks = chunk.Blocks;
        int size = Chunk.Size;
        if(File.Exists(path)) File.Delete(path);
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        FileStream fs = new FileStream(path, FileMode.Create);
        
        for (int i = 0; i < size; i++)
        for (int j = 0; j < size; j++)
        for (int k = 0; k < size; k++)
        {
            int currBlock = blocks[i, j, k];
            
            // saving as 16bit number, we don't need more than 127 blocks types for now
            
            //fs.WriteByte((byte)((currBlock >> 24) & 0xFF));   //uncomment for 32bit saves ( 2^32 types )
            //fs.WriteByte((byte)((currBlock >> 16) & 0xFF));   //uncomment for 24bit saves ( 2^24 types )
            //fs.WriteByte((byte)((currBlock >> 8) & 0xFF));    //uncomment for 16bit saves ( 2^16 types )
            fs.WriteByte((byte)(currBlock & 0xFF));             //uncomment for  8bit saves ( 256 types )
        }
        fs.Close();
    }
    
    private int LoadChunksMesh(Chunk chunk)
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
            
            //currBlock |= fs.ReadByte() << 24;     //uncomment for 32bit saves ( 2^32 types )
            //currBlock |= fs.ReadByte() << 16;     //uncomment for 24bit saves ( 2^24 types )
            //currBlock |= fs.ReadByte() << 8;      //uncomment for 16bit saves ( 2^16 types )
            currBlock |= fs.ReadByte();             //uncomment for  8bit saves ( 256 types )
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
        GameObject globals = GameObject.Find("SuperGlobals"); // just to know if we started from main menu
        if (globals != null) Game.SaveManager.SaveName = SuperGlobals.SaveName;
    }
    
    [Command (requiresAuthority = false)]
    private void RequestChunk(int x, int z, int id, bool last) // running on the server
    {
        int[,,] blocks = GameObject.Find(x + "." + z).GetComponent<Chunk>().Blocks;
        int len = blocks.Length;
        int[] bytes = new int[len];
        Buffer.BlockCopy(blocks, 0, bytes, 0, len * 4);
        
        SendChunk(bytes, x, z, id, last);

    }
    
    [ClientRpc]
    private void SendChunk(int[] bytes, int x, int z, int id, bool last)   // running on the client
    {
        if (NetworkClient.localPlayer.GetInstanceID() != id) return;
        
        int[,,] blocks = new int[Chunk.Size,Chunk.Size,Chunk.Size];
        Buffer.BlockCopy(bytes, 0, blocks, 0, bytes.Length*4);
        
        Vector3 pos = new Vector3(x, 0, z);
        GameObject chunkObject = Instantiate(chunkPlane, _chunksParent);
        chunkObject.name = pos.x + "." + pos.z;
        
        Chunk chunk = chunkObject.GetComponent<Chunk>();
        MeshRenderer meshRenderer = chunkObject.GetComponent<MeshRenderer>();
        meshRenderer.material = material;
        
        chunk.Blocks = blocks;

        Chunks.TryAdd(chunkObject.name, chunk);
        chunk.Init(pos, true);
        
        if (SuperGlobals.IsNewSave && last) Game.Player.SetSpawn(new Vector3(0, Chunk.Size, 0));
    }
}