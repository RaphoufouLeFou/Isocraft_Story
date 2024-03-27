using System.IO;
using UnityEngine;

// static class for chunks saving utilities
public static class ChunksSave
{
    private static Region _region;

    public static void SaveAllChunks()
    {
        foreach (Chunk chunk in MapManagement.Chunks.Values) SaveChunk(chunk);
    }

    private static void SaveChunk(Chunk chunk)
    {
        if (SuperGlobals.EditorMode) return;
        if (Game.Player.isClient) throw new NetworkException("Clients aren't allowed to save chunks");

        string dirPath = $"{Application.persistentDataPath}/Saves/{SuperGlobals.SaveName}/Chunks/";
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

    public static bool LoadBlocks(int[,,] blocks, string chunkName)
    {
        // try to retrieve the blocks from the world save, then return true if successful and false if not

        if (SuperGlobals.EditorMode) return false;

        string path = $"{Application.persistentDataPath}/Saves/{SuperGlobals.SaveName}/Chunks/{chunkName}.Chunk";
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
}
