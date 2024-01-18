using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Structure
{
    public readonly int X, Y, Z;
    private readonly int[,,] _blocks;
    public readonly (int x, int y, int z) Offset;
    private string _type;

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
                _blocks[i % X, i / X / Z, i / X % Z] = b;
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
                return NoiseGen.PrngPos(x + dx, y + dy, z + dz) < 0.5f ? Game.Blocks.OakLeaves : Game.Blocks.None;
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
    [NonSerialized] public static int Level = 0;
    [NonSerialized] public static int Seed;
    [NonSerialized] public static string SaveName;

    // structures info
    public static readonly Dictionary<string, Structure> Structs = new();
    private static readonly string[] StructNames = { "Tree" };
    [NonSerialized] public static int MaxStructSize; // how many blocks out can structures be searched for

    static class Tiles
    {
        public static readonly Tile
            Bedrock = new(new Vector2(0, 2)),
            Cobblestone = new(new Vector2(1, 2)),
            OakLeaves = new(new Vector2(2, 2)),
            OakLog = new(new Vector2(3, 2)),
            OakLogTop = new(new Vector2(0, 1)),
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
            OakLog = 6,
            OakLeaves = 7;

        public static readonly Dictionary<int, Block> FromId = new()
        {
            {Air, new Block()},
            {Sand, new Block(Sand, Tiles.SandTop, Tiles.SandSide, Tiles.SandTop)},
            {RedSand, new Block(RedSand, Tiles.RedSand)},
            {Sandstone, new Block(Sandstone, Tiles.SandstoneTop, Tiles.SandstoneSide, Tiles.SandstoneTop)},
            {Bedrock, new Block(Bedrock, Tiles.Bedrock)},
            {Cobblestone, new Block(Cobblestone, Tiles.Cobblestone)},
            {OakLog, new Block(OakLog, Tiles.OakLogTop, Tiles.OakLog, Tiles.OakLogTop)},
            {OakLeaves, new Block(OakLeaves, Tiles.OakLeaves)}
        };
    }

    public void SaveGame()
    {
        if(SaveName == "") return;
        string path = Application.persistentDataPath + "/Saves/";
        Directory.CreateDirectory(path);
        path += SaveName + ".IsoSave";
        File.WriteAllText(path, "Ceci est un test");
    }

    void Awake()
    {
        SaveName = SaveInfos.SaveName;
        SaveGame();
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
}