using UnityEngine;

public enum Tag
{
    Transparent = 0,
    Unbreakable = 1,
    Is2D = 2,
    NoCollide = 3,
    NoTexture = 4,
    CanInteract = 5
}

public class Block
{
    // tags
    public readonly bool Transparent;
    public readonly bool Unbreakable;
    public readonly bool Is2D;
    public readonly bool NoCollide;
    public readonly bool NoTexture;
    public readonly bool CanInteract;

    private readonly Tiles.Tile _top, _sides, _bottom;

    private Block(Tag[] tags = null)
    {
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
                case Tag.NoTexture:
                    Unbreakable = true;
                    Transparent = true;
                    NoTexture = true;
                    break;
                case Tag.Interactable:
                    CanInteract = true;
                    break;
                default: throw new BlockException($"Unknown tag name: {tag}");
            }

        if (Is2D && !Transparent) throw new BlockException("2D blocks must be transparent");
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