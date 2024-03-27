using System;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [NonSerialized] public static float TickRate = 20;
    [NonSerialized] public static int Tick;
    [NonSerialized] public static int Seed;
    [NonSerialized] public static bool Started;
    [NonSerialized] public static float InvincibilityTime = 0.5f;

    // static variables, get initialized from their serialized variables
    [NonSerialized] public static Sprite[] InvSprites;
    [NonSerialized] public static Player Player;
    [NonSerialized] public static SaveManagement SaveManager;

    public Sprite[] sprites;
    public MapManager mapManager;

    public GameObject[] models;

    private static Game _object; // non-static Game object, available after InitGameUtils

    public static Game Object
    {
        get
        {
            if (_object is null)
                throw new NullExceptionCrash("Attempt at getting Game.Object before initialization");
            return Object;
        }

        private set => _object = value;
    }

    private float _prevTick;
    private float _prevSave;

    // structures info
    public static readonly Dictionary<string, Structure> Structs = new();
    private static readonly string[] StructNames = { "Tree", "Trunk", "Bush" };
    [NonSerialized] public static int MaxStructSize; // how many blocks out can structures be searched for

    public struct Blocks
    {
        public const int
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
            DeadPlant = 15,
            Chest = 16;

        public static readonly Dictionary<int, Block> FromId = new()
        {
            { Air, new Block(null, new[] { Tag.NoTexture, Tag.NoCollide, Tag.NoRayCast, Tag.Unbreakable }) },
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
            { DeadBush, new Block(Tiles.DeadBush, new[] { Tag.Is2D, Tag.NoCollide }) },
            { DeadPlant, new Block(Tiles.DeadPlant, new[] { Tag.Is2D, Tag.NoCollide }) },
            { Chest, new Block(null, new[] { Tag.IsModel }) }
        };
    }

    public struct Models
    {
        // { Block ID, asset index }
        public static readonly Dictionary<int, int> ModelsIndex = new()
        {
            { Blocks.Chest, 0 }
        };

        public static GameObject[] GameObjects;
    }

    public struct Mobs
    {
        public const int
            PlayerMob = 0,
            Mob = 1,
            Zapatos = 2;

        public static readonly Dictionary<int, string> Prefab = new()
        {
            { PlayerMob, "Player" },
            { Mob, "Mob" },
            { Zapatos, "Zapatos" }
        };

        public static readonly Dictionary<int, string> Names = new()
        {
            { PlayerMob, "Player" },
            { Mob, "Mob" },
            { Zapatos, "Zapatos" }
        };

        public static readonly Dictionary<int, int> Health = new()
        {
            { PlayerMob, 100 },
            { Mob, 9001 },
            { Zapatos, 50 }
        };
    }

    public void InitGameUtils()
    {
        if (Models.ModelsIndex.Count != models.Length) throw new BlockException("Not all models initialized");
        Models.GameObjects = models;
        Object = this;

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
    }

    private void StartGame()
    {
        mapManager.StartMapHandle();
        // local player hasn't been spawned by map handler
        if (SuperGlobals.IsNewSave) Player.SetSpawn(Player.transform.position);

        Tick = 0;
        _prevTick = Time.time;
    }

    private void Update()
    {
        // wait for the player to start
        if (!Started && Player is not null && Player.IsLoaded)
        {
            // current player has been fully loaded
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
        }
    }

    public static void QuitGame()
    {
        // quit game, even if in editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
