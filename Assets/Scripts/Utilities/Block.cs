using System;
using UnityEngine;

public enum Tag
{
    Transparent = 0,
    Unbreakable = 1,
    Is2D = 2,
    NoCollide = 3
}

public class Block
{
    public readonly int Id;
    
    // tags
    public bool Transparent;
    public bool Unbreakable;
    public bool Is2D;
    public bool NoCollide;

    private readonly Tiles.Tile _top, _sides, _bottom;

    private Block(int id, Tag[] tags = null) // block without textures
    {
        Id = id;
        if (tags != null) foreach(Tag tag in tags)
            switch (tag)
            {
                case Tag.Transparent:
                    Transparent = true;
                    break;
                case Tag.Unbreakable:
                    Unbreakable = true;
                    break;
                case Tag.Is2D:
                    Is2D = true;
                    break;
                case Tag.NoCollide:
                    NoCollide = true;
                    break;
                default: throw new BlockException($"Unknown tag name: {tag}");
            }
    }

    public Block(int id, Tiles.Tile allFaces, Tag[] tags = null) : this(id, tags)
    {
        if (allFaces != null) _top = _sides = _bottom = allFaces;
    }

    public Block(int id, Tiles.Tile top, Tiles.Tile sides, Tiles.Tile bottom, Tag[] tags = null) : this(id, tags)
    {
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