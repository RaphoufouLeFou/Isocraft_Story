using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public enum Tag
{
    Transparent = 0,
    Unbreakable = 1,
    Is2D = 2,
    IsModel = 3,
    NoCollide = 4,
    NoTexture = 5,
    IsFluid = 6,
    HasInfo = 7,
    CanInteract = 8
}

public class Block
{
    // tags
    public readonly bool Transparent;
    public readonly bool Unbreakable;
    public readonly bool Is2D;
    public readonly bool IsModel;
    public readonly bool NoCollide;
    public readonly bool NoTexture;
    public readonly bool IsFluid;
    public readonly bool HasInfo;
    public readonly bool CanInteract;

    public readonly Tag[] Tags; // deprecated, use individual properties instead

    public readonly Dictionary<string, int> Info;

    private readonly Tiles.Tile _top, _sides, _bottom;

    private Block(Tag[] tags = null)
    {
        Tags = tags;
        
        if (tags != null) foreach (Tag tag in tags)
            switch (tag)
            {
                case Tag.Transparent:
                    if (NoTexture || IsModel) Warn("Transparent", "NoTexture");
                    Transparent = true;
                    break;
                case Tag.Unbreakable:
                    Unbreakable = true;
                    break;
                case Tag.Is2D:
                    Is2D = true;
                    Transparent = true;
                    break;
                case Tag.NoCollide:
                    NoCollide = true;
                    break;
                case Tag.NoTexture:
                    if (Transparent) Warn("Transparent", "NoTexture");
                    if (IsModel) Warn("NoTexture", "IsModel");
                    Transparent = true;
                    NoTexture = true;
                    break;
                case Tag.IsFluid:
                    IsFluid = true;
                    break;
                case Tag.IsModel:
                    if (NoTexture) Warn("NoTexture", "IsModel");
                    if (Transparent) Warn("Transparent", "IsModel");
                    IsModel = true;
                    NoTexture = true;
                    Transparent = true;
                    break;
                case Tag.HasInfo:
                    HasInfo = true;
                    break;
                case Tag.CanInteract:
                    CanInteract = true;
                    break;
                default: throw new BlockException($"Unknown tag name: {tag}");
            }

        if (HasInfo) Info = new Dictionary<string, int>();
    }

    private void Warn(string useless, string child)
    {
        throw new BlockException($"{useless} is not required when specifying {child}, please remove for performance");
    }

    public Block(Tiles.Tile allFaces, Tag[] tags = null) : this(tags)
    {
        if (allFaces != null) _top = _sides = _bottom = allFaces;
        else if (!NoTexture) throw new BlockException("Block without texture must be tagged NoTexture");
    }

    public Block(Tiles.Tile top, Tiles.Tile sides, Tiles.Tile bottom, Tag[] tags = null) : this(tags)
    {
        _top = top;
        _sides = sides;
        _bottom = bottom;
    }

    public Vector2[] GetUVs(int faceIndex)
    {
        if (_sides == null)
        {
            // get id
            string id = null;
            foreach((int key, Block value) in Game.Blocks.FromId)
                if (value == this)
                {
                    id = key.ToString();
                    break;
                }
            throw new BlockException($"Trying to access texture from block of ID: {id ?? "[unknown]"}");
        }
        
        switch (faceIndex)
        {
            case 2: return _top.UVs;
            case 3: return _bottom.UVs;
            default: return _sides.UVs;
        }
    }
}