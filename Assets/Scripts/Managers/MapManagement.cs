using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Mirror;
using TMPro;

public class MapManagement : NetworkBehaviour
{
    public GameObject chunkPlane;
    public GameObject chunkParent;
    private Transform _chunksParent;

    private readonly Queue<(GameObject, int, int, int[,,])> _chunkQueue = new();

    private const int ChunkUpdatesPerFrame = 4;

    private int? _id;
    private int Id
    {
        get
        {
            _id ??= NetworkClient.localPlayer.GetInstanceID();
            return (int)_id;
        }
    }

    [NonSerialized] public static Dictionary<string, Chunk> Chunks;

    public void StartMapHandle()
    {
        _chunksParent = chunkParent.transform;
        Chunks = new Dictionary<string, Chunk>();
        transform.position = new Vector3(0, 0, 0);

        for (int x = -4; x < 5; x++)
            for (int z = -4; z < 5; z++)
                CmdGenChunk(x, z, Id);
        CmdRequestPlayersNameDictionary(NetworkClient.localPlayer.GetInstanceID(), NetworkClient.localPlayer.assetId,
            SuperGlobals.PlayerName);
        if (isServer)
        {
            if (SuperGlobals.IsNewSave) Game.SaveManager.SaveGame(); // initial save
            else Game.SaveManager.LoadSave();
        }

        else CmdRequestGameName(Id);
    }

    [Command (requiresAuthority = false)]
    private void CmdRequestGameName(int id)
    {
        RpcSendGameName(SuperGlobals.SaveName, id);
    }

    [ClientRpc]
    private void RpcSendGameName(string gameName, int id)
    {
        if (NetworkClient.localPlayer.GetInstanceID() != id) return;

        SuperGlobals.SaveName = gameName;
        string clientName = $"CLIENT__{gameName}";
        string path = $"{Application.persistentDataPath}/Saves/{clientName}/{clientName}.IsoSave";

        if (!File.Exists(path)) Game.SaveManager.SaveGame();
        else Game.SaveManager.LoadSave();
    }

    [Command(requiresAuthority = false)]
    private void CmdRequestPlayersNameDictionary(int id, uint netId, string playerName)
    {
        bool res = Game.PlayersNames.TryAdd(netId, playerName);
        if (res) Debug.LogWarning("A player already exist with this name !");
        List<uint> idList = new List<uint>();
        List<string> nameList = new List<string>();
        foreach (KeyValuePair<uint, string> keyValuePair in Game.PlayersNames)
        {
            idList.Add(keyValuePair.Key);
            nameList.Add(keyValuePair.Value);
        }

        RpcGetPlayersNameDictionary(idList, nameList, res, id);
    }

    [ClientRpc]
    private void RpcGetPlayersNameDictionary(List<uint> idList, List<string> nameList, bool nameExist, int id)
    {
        if (NetworkClient.localPlayer.GetInstanceID() == id)
            if(!nameExist) GameObject.Find("NetworkManagement").GetComponent<NetworkManagement>().LeaveGame();
        Dictionary<uint, string> dictionary = new Dictionary<uint, string>();
        for (int i = 0; i < idList.Count; i++)
            dictionary.Add(idList[i], nameList[i]);

        Game.PlayersNames = dictionary;
        //UpdateNameTags(); Not working yet
    }

    private void UpdateNameTags()
    {
        foreach (KeyValuePair<uint, string> pair in Game.PlayersNames)
        {
            NetworkServer.spawned[pair.Key].transform.GetChild(3).GetComponent<TMP_Text>().text = pair.Value;
        }
    }

    [Command (requiresAuthority = false)]
    private void CmdGenChunk(int cx, int cz, int id)
    {
        // prepare a chunk, or load it, to send its blocks to a client

        int[,,] blocks = new int[Chunk.Size, Chunk.Size, Chunk.Size];
        string chunkName = $"{cx}.{cz}";

        // chunk is loaded on the server
        if (Chunks.TryGetValue(chunkName, out Chunk chunk)) blocks = chunk.Blocks;
        // chunk isn't loaded
        else if (!ChunksSave.LoadBlocks(blocks, chunkName)) // try to get blocks from save, otherwise generate
            Chunk.GenerateBlocks(blocks, cx * Chunk.Size, cz * Chunk.Size);

        int len = blocks.Length;
        int[] bytes = new int[len];
        Buffer.BlockCopy(blocks, 0, bytes, 0, len * 4);

        RpcGetBlocks(bytes, cx, cz, id);
    }

    [ClientRpc]
    private void RpcGetBlocks(int[] bytes, int cx, int cz, int id)
    {
        // clients generate chunks here
        if (NetworkClient.localPlayer.GetInstanceID() != id) return; // only send to a specific client

        int[,,] blocks = new int[Chunk.Size, Chunk.Size, Chunk.Size];
        Buffer.BlockCopy(bytes, 0, blocks, 0, bytes.Length * 4);

        _chunkQueue.Enqueue((Instantiate(chunkPlane, _chunksParent), cx, cz, blocks));
    }

    private void Update()
    {
        // update chunks queue, don't load them asynchronously from each other (would mess with neighbors checking)
        int count = 0;
        while (_chunkQueue.Count != 0 && count++ < ChunkUpdatesPerFrame)
        {
            (GameObject plane, int cx, int cz, int[,,] blocks) = _chunkQueue.Dequeue();
            Chunk chunk = plane.GetComponent<Chunk>();
            chunk.Init(cx, cz, blocks);
        }
    }
}
