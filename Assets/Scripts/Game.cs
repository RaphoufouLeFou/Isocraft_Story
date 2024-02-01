using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Random = System.Random;

public class Structure
{
    public readonly int X, Y, Z;
    private readonly int[,,] _blocks;
    public readonly (int x, int y, int z) Offset;
    private readonly string _type;

    private string[] GetDataLine(StreamReader file)
    {
        string line = file.ReadLine();
        if (line == null) throw new Exception();
        return line.Split(".");
    }

    public Structure()
    {
        // dummy constructor, no data
    }

    public Structure(string type)
    {
        _type = type;
        try
        {
            StreamReader file = new StreamReader("Assets/Structures/" + type + ".txt");
            // first line: size (x.y.z)
            string[] coords = GetDataLine(file);
            X = int.Parse(coords[0]);
            Y = int.Parse(coords[1]);
            Z = int.Parse(coords[2]);
            _blocks = new int[X, Y, Z];

            // second line: origin for height offset: x/z=origin, y=offset (x.y.z)
            string[] origin = GetDataLine(file);
            Offset = (int.Parse(origin[0]), int.Parse(origin[1]), int.Parse(origin[2]));
            
            // third line: blocks (x, then z, then y, separated by .)
            string[] data = GetDataLine(file);
            if (data.Length != X * Y * Z) throw new Exception();
            for (int i = 0; i < data.Length; i++)
            {
                int b = data[i] == "" ? -1 : int.Parse(data[i]); // empty value: -1 (ignore)
                _blocks[i / (Y * Z), i / Z % Y, i % Z] = b;
            }
        }
        catch
        {
            throw new ArgumentException("Error loading structure \"" + type + "\"");
        }
    }
    
    public int GetBlock(int x, int y, int z, int dx, int dy, int dz)
    {
        // used to add additional generation conditions to structures
        int b = _blocks[dx, dy, dz];
        if (_type == "Tree")
        {
            if ((dx, dy, dz) == (1, 0, 1)) return Game.Blocks.RedSand;
            if (dx != 1 && dy == 5 && dz != 1 && b == -1)
                return NoiseGen.PrngPos(x + dx, y + dy, z + dz) < 0.5f ? Game.Blocks.DesertLeaves : Game.Blocks.None;
        }
        return b;
    }
}

public static class Utils
{
    public static int Mod(int a, int b)
    {
        return (a % b + b) % b;
    }
    
    public static int Floor(float x)
    {
        if (x < 0)
        {
            int offset = 1 - (int)x;
            return (int)(x + offset) - offset;
        }

        return (int)x;
    }
}

public class Tile
{
    public readonly Vector2[] UVs;
    private readonly int _texWidth = 4, _texHeight = 3;
    private readonly float _offset = 0.5f / 32f; // prevent texture bleeding

    public Tile(Vector2 pos)
    {
        UVs = new Vector2[]
        {
            new((pos.x + _offset) / _texWidth, (pos.y + _offset) / _texHeight),
            new((pos.x + _offset) / _texWidth, (pos.y + 1 - _offset) / _texHeight),
            new((pos.x + 1 - _offset) / _texWidth, (pos.y + 1 - _offset) / _texHeight),
            new((pos.x + 1 - _offset) / _texWidth, (pos.y + _offset) / _texHeight)
        };
    }
}

public class Block
{
    public readonly int Id;

    private readonly Tile _top, _sides, _bottom;

    public Block() // air block
    {
        Id = 0;
    }

    public Block(int id, Tile allFaces)
    {
        Id = id;
        _top = _sides = _bottom = allFaces;
    }

    public Block(int id, Tile top, Tile sides, Tile bottom)
    {
        Id = id;
        _top = top;
        _sides = sides;
        _bottom = bottom;
    }

    public Vector2[] GetUVs(int faceIndex)
    {
        switch (faceIndex)
        {
            case 2: return _top.UVs;
            case 3: return _bottom.UVs;
            default: return _sides.UVs;
        }
    }
}

public class Game : MonoBehaviour
{
    [NonSerialized] public static float TickRate = 20;
    [NonSerialized] public static int Tick;
    [NonSerialized] public static int Level = 0;
    [NonSerialized] public static int Seed;
    [NonSerialized] public static string SaveName;
    
    private float _prevTime;
    
    public Sprite[] sprites;
    private bool _autoSave = true;

    // structures info
    public static readonly Dictionary<string, Structure> Structs = new();
    private static readonly string[] StructNames = { "Tree", "Trunk", "Bush", "Penis" };
    [NonSerialized] public static int MaxStructSize; // how many blocks out can structures be searched for

    static class Tiles
    {
        public static readonly Tile
            Bedrock = new(new Vector2(0, 2)),
            Cobblestone = new(new Vector2(1, 2)),
            DesertLeaves = new(new Vector2(2, 2)),
            DesertLog = new(new Vector2(3, 2)),
            DesertLogTop = new(new Vector2(0, 1)),
            RedSand = new(new Vector2(1, 1)),
            SandstoneSide = new(new Vector2(2, 1)),
            SandstoneTop = new(new Vector2(3, 1)),
            SandSide = new(new Vector2(0, 0)),
            SandTop = new(new Vector2(1, 0));
    }

    public static class Blocks
    {
        public static readonly int
            None = -1,
            Air = 0,
            Sand = 1,
            RedSand = 2,
            Sandstone = 3,
            Bedrock = 4,
            Cobblestone = 5,
            DesertLog = 6,
            DesertLeaves = 7;

        public static readonly Dictionary<int, Block> FromId = new()
        {
            {Air, new Block()},
            {Sand, new Block(Sand, Tiles.SandTop, Tiles.SandSide, Tiles.SandTop)},
            {RedSand, new Block(RedSand, Tiles.RedSand)},
            {Sandstone, new Block(Sandstone, Tiles.SandstoneTop, Tiles.SandstoneSide, Tiles.SandstoneTop)},
            {Bedrock, new Block(Bedrock, Tiles.Bedrock)},
            {Cobblestone, new Block(Cobblestone, Tiles.Cobblestone)},
            {DesertLog, new Block(DesertLog, Tiles.DesertLogTop, Tiles.DesertLog, Tiles.DesertLogTop)},
            {DesertLeaves, new Block(DesertLeaves, Tiles.DesertLeaves)}
        };
    }

    public static float SmoothStep(float t)
    {
        return (3 - 2 * t) * t * t;
    }
    
    public void SaveGame()
    {
        if(SaveName == "") return;
        string path = Application.persistentDataPath + "/Saves/" + SaveName + "/" + SaveName + ".IsoSave";
        string text =
            "PlayerX:" + SaveInfos.PlayerPosition.x + "\n" +
            "PlayerY:" + SaveInfos.PlayerPosition.y + "\n" +
            "PlayerZ:" + SaveInfos.PlayerPosition.z + "\n" +
            "RotationX:" + SaveInfos.PlayerRotation.x + "\n" +
            "RotationY:" + SaveInfos.PlayerRotation.y + "\n" +
            "RotationZ:" + SaveInfos.PlayerRotation.z + "\n";
        
        for (int j = 0; j < 4; j++) 
        for (int i = 0; i < 9; i++) 
            text += "Inv" + i + "" + j + ":" + SaveInfos.PlayerInventory.GetCurrentBlock(i, j) + "." +
                    SaveInfos.PlayerInventory.GetCurrentBlockCount(i, j) + "\n";
        
        if(File.Exists(path)) File.Delete(path);
        File.WriteAllText(path, text);
        
            
    }
    
    private void CreateSaveFile()
    {
        if(SaveName == "") return;
        string path = Application.persistentDataPath + "/Saves/" + SaveName + "/";
        Directory.CreateDirectory(path);
        string mainSave = path + SaveName + ".IsoSave";
        if(!File.Exists(mainSave)) File.WriteAllText(mainSave, "");
        string chunkSave = path + "Chunks/";
        if (!Directory.Exists(chunkSave)) Directory.CreateDirectory(chunkSave);
        StartCoroutine(AutoSave());
    }

    private void LoadSave()
    {
        SaveInfos.HasBeenLoaded = false;
        if(SaveName == "") return;
        string path = Application.persistentDataPath + "/Saves/" + SaveName + "/" + SaveName + ".IsoSave";
        if(!File.Exists(path)) return;
        Debug.Log("Loading save " + SaveName);
        StreamReader file = new StreamReader(path);

        SaveInfos.PlayerInventory = new ();
        float posX = 0, posY = 0, posZ = 0;
        float rotX = 0, rotY = 0, rotZ = 0;
        
        while (file.ReadLine() is { } line)
        {
            // check if valid line and edit settings
            int i;
            for (i = 0; i < line.Length; i++) if (line[i] == ':') break;
            if (i == line.Length) continue;
            string key = line.Substring(0, i), value = line.Substring(i + 1);
            if (key == "PlayerX") posX = float.Parse(value);
            else if (key == "PlayerY")  posY = float.Parse(value);
            else if (key == "PlayerZ")  posZ = float.Parse(value);
            else if (key == "RotationX")  rotX = float.Parse(value);
            else if (key == "RotationY")  rotY = float.Parse(value);
            else if (key == "RotationZ")  rotZ = float.Parse(value);
            else if (key.Contains("Inv"))
            {
                int x = key[3] - '0';
                int y = key[4] - '0';
                int index = value.IndexOf('.');
                int type = Int32.Parse(value.Substring(0, index));
                int count = Int32.Parse(value.Substring(index + 1));
                
                if (count > 0 && type > 0) SaveInfos.PlayerInventory.AddBlockAt(x, y, type, count);
            }

        }
        
        SaveInfos.PlayerPosition = new Vector3(posX, posY, posZ);
        SaveInfos.PlayerRotation = new Vector3(
            rotX,
            Mathf.Round(rotY / 45) * 45,
            rotZ
            );
        SaveInfos.HasBeenLoaded = true;
        file.Close();
    }

    public void StartGame()
    {
        SaveName = SaveInfos.SaveName;

        LoadSave();
        
        Random rand = new Random();
        Seed = (int)rand.NextDouble();
        
        // initialize static classes
        NoiseGen.Init();
        
        // initialize structures
        foreach (string type in StructNames)
        {
            Structure s = new Structure(type);
            MaxStructSize = MaxStructSize > s.X ? MaxStructSize > s.Z ? MaxStructSize : s.Z : s.X > s.Z ? s.X : s.Z;
            Structs.TryAdd(type, s);
        }
    }

    private void Start()
    {
        GetComponentInChildren<MapHandler>().StartMapHandle();
        CreateSaveFile();
        Tick = 0;
    }

    private void Update()
    {
        // tick
        if (Time.time - _prevTime > 1 / TickRate)
        {
            _prevTime = Time.time;
            Tick++;
        }
    }

    void OnApplicationQuit()
    {
        _autoSave = false;
        SaveGame();
    }
    private IEnumerator AutoSave()
    {
        while (_autoSave)
        {
            yield return new WaitForSecondsRealtime(1);
            SaveGame();
        }
    }
}