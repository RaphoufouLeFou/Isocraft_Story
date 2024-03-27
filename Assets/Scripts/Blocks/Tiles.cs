using UnityEngine;

public static class Tiles
{
    public class Tile
    {
        public readonly Vector2[] UVs;
        private const int TexWidth = 5, TexHeight = 4;
        private readonly float _offset = 0.5f / 32f; // prevent texture bleeding

        public Tile(Vector2 pos)
        {
            UVs = new Vector2[]
            {
                new((pos.x + _offset) / TexWidth, (pos.y + _offset) / TexHeight),
                new((pos.x + _offset) / TexWidth, (pos.y + 1 - _offset) / TexHeight),
                new((pos.x + 1 - _offset) / TexWidth, (pos.y + 1 - _offset) / TexHeight),
                new((pos.x + 1 - _offset) / TexWidth, (pos.y + _offset) / TexHeight)
            };
        }
    }

    public static readonly Tile
        Bedrock = new(new Vector2(0, 3)),
        BokaBeast = new(new Vector2(1, 3)),
        BokaBoom = new(new Vector2(2, 3)),
        BokaBrick = new(new Vector2(3, 3)),
        BokaConquer = new(new Vector2(4, 3)),
        BokaFear = new(new Vector2(0, 2)),
        BokaHome = new(new Vector2(1, 2)),
        Cobblestone = new(new Vector2(2, 2)),
        DeadBush = new(new Vector2(3, 2)),
        DeadPlant = new(new Vector2(4, 2)),
        DesertLeaves = new(new Vector2(0, 1)),
        DesertLog = new(new Vector2(1, 1)),
        DesertLogTop = new(new Vector2(2, 1)),
        RedSand = new(new Vector2(3, 1)),
        SandstoneSide = new(new Vector2(4, 1)),
        SandstoneTop = new(new Vector2(0, 0)),
        SandSide = new(new Vector2(1, 0)),
        SandTop = new(new Vector2(2, 0));
}
