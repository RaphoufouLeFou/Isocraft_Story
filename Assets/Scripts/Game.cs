﻿using System;
using System.IO;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

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
            if (dx != 1 && dy == 5 && dz != 1 && b == -1)
                return NoiseGen.PrngPos(x + dx, y + dy, z + dz) < 0.5f ? Game.Blocks.DesertLeaves : Game.Blocks.None;
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

    public static float SmoothStep(float t)
    {
        return (3 - 2 * t) * t * t;
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
    [NonSerialized] public static bool Started;
    
    // static variables, get initialized from their serialized variables
    [NonSerialized] public static Sprite[] InvSprites;
    [NonSerialized] public static Player Player;
    [NonSerialized] public static SaveManagement SaveManager;

    public Sprite[] sprites;
    public MapHandler mapHandler;
    
    private float _prevTick;
    private float _prevSave;
    private const int AutoSaveDelay = 15;

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

    public void InitGameUtils()
    {
        // start some things very early
        Started = false;
        SaveManager = new();
        InvSprites = sprites;
        HotBar.InitImages();
        Inventory.Init();
        System.Random rand = new System.Random();
        Seed = (int)rand.NextDouble();
        NoiseGen.Init();
        
        // initialize structures
        foreach (string type in StructNames)
        {
            Structure s = new Structure(type);
            MaxStructSize = MaxStructSize > s.X ? MaxStructSize > s.Z ? MaxStructSize : s.Z : s.X > s.Z ? s.X : s.Z;
            Structs.TryAdd(type, s);
        }

        // set up SuperGlobals
        GameObject globals = GameObject.Find("SuperGlobals"); // just to know if we started from main menu
        if (globals != null) SaveManager.SaveName = SuperGlobals.SaveName;
    }
    
    private void StartGame()
    {
        mapHandler.StartMapHandle();
        if (SuperGlobals.IsNewSave) SaveManager.SaveGame(); // initial save
        else SaveManager.LoadSave();
        Tick = 0;
        _prevTick = Time.time;
    }

    private void Update()
    {
        // wait for the player to start
        if (!Started && Player is not null && Player.IsLoaded) // local player joined!
        {
            StartGame();
            Started = true;
        }
        
        // tick
        if (Time.time - _prevTick > 1 / TickRate)
        {
            _prevTick = Time.time;
            Tick++;
        }
        
        // autoSave
        if (Time.time - _prevSave > AutoSaveDelay && Player is not null)
        {
            _prevSave = Time.time;
            SaveManager.SaveGame();
        }
    }


}