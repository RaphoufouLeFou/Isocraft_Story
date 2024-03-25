using System;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [NonSerialized] public static float TickRate = 20;
    [NonSerialized] public static int Tick;
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

    // structures info
    public static readonly Dictionary<string, Structure> Structs = new();
    private static readonly string[] StructNames = { "Tree", "Trunk", "Bush" };
    [NonSerialized] public static int MaxStructSize; // how many blocks out can structures be searched for

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
            DesertLeaves = 7,
            BokaBrick = 8,
            BokaConquer = 9,
            BokaFear = 10,
            BokaBoom = 11,
            BokaHome = 12,
            BokaBeast = 13,
            DeadBush = 14,
            DeadPlant = 15;

        public static readonly Dictionary<int, Block> FromId = new()
        {
            { Air, new Block(null, new[] { Tag.NoTexture, Tag.NoCollide }) },
            { Sand, new Block(Tiles.SandTop, Tiles.SandSide, Tiles.SandTop) },
            { RedSand, new Block(Tiles.RedSand) },
            { Sandstone, new Block(Tiles.SandstoneTop, Tiles.SandstoneSide, Tiles.SandstoneTop) },
            { Bedrock, new Block(Tiles.Bedrock, new[] { Tag.Unbreakable }) },
            { Cobblestone, new Block(Tiles.Cobblestone) },
            { DesertLog, new Block(Tiles.DesertLogTop, Tiles.DesertLog, Tiles.DesertLogTop) },
            { DesertLeaves, new Block(Tiles.DesertLeaves, new[] { Tag.Transparent }) },
            { BokaBrick, new Block(Tiles.BokaBrick) },
            { BokaConquer, new Block(Tiles.BokaBrick, Tiles.BokaConquer, Tiles.BokaBrick) },
            { BokaFear, new Block(Tiles.BokaBrick, Tiles.BokaFear, Tiles.BokaBrick) },
            { BokaBoom, new Block(Tiles.BokaBrick, Tiles.BokaBoom, Tiles.BokaBrick) },
            { BokaHome, new Block(Tiles.BokaBrick, Tiles.BokaHome, Tiles.BokaBrick) },
            { BokaBeast, new Block(Tiles.BokaBrick, Tiles.BokaBeast, Tiles.BokaBrick) },
            { DeadBush, new Block(Tiles.DeadBush, new[] { Tag.Is2D, Tag.Transparent, Tag.NoCollide }) },
            { DeadPlant, new Block(Tiles.DeadPlant, new[] { Tag.Is2D, Tag.Transparent, Tag.NoCollide }) }
        };
    }

    public void InitGameUtils()
    {
        // start some things very early
        Started = false;
        
        InvSprites = sprites;
        HotBar.InitImages();
        Inventory.Init();
        System.Random rand = new System.Random();
        Seed = (int)(rand.NextDouble() * (1L << 16));
        NoiseGen.Init();
        
        // initialize structures
        foreach (string type in StructNames)
        {
            Structure s = new Structure(type);
            MaxStructSize = MaxStructSize > s.X ? MaxStructSize > s.Z ? MaxStructSize : s.Z : s.X > s.Z ? s.X : s.Z;
            Structs.TryAdd(type, s);
        }

        SaveManager = new SaveManagement();
        // set up SuperGlobals
        GameObject globals = GameObject.Find("SuperGlobals"); // just to know if we started from main menu
        if (globals != null) SaveManager.SaveName = SuperGlobals.SaveName;
    }
    
    private void StartGame()
    {
        mapHandler.StartMapHandle();
        // local player hasn't been spawned by map handler
        if (SuperGlobals.IsNewSave) Player.SetSpawn(Player.transform.position);

        Tick = 0;
        _prevTick = Time.time;
    }

    private void Update()
    {
        // wait for the player to start
        if (!Started && Player is not null && Player.IsLoaded) // local player has been fully loaded
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
        if (Time.time - _prevSave > Settings.Game.AutoSaveDelay && Player is not null)
        {
            _prevSave = Time.time;
            SaveManager.SaveGame();
            MapHandler.SaveAllChunks();
        }
    }

    public static void QuitGame()
    {
        // quit game, even if in editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("Game quit in editor");
#endif
        Application.Quit();
    }
}