using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

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
    [NonSerialized] public static long Seed;

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

    void Start()
    {
        Random rand = new Random();
        Seed = (long)rand.NextDouble();
        
        // initialize static classes
        NoiseGen.Init();
        Structures.Init();
    }
}