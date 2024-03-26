using System.Collections.Generic;

public static class NoiseGen
{
    private static FastNoiseLite _simplex, _value, _value2;
    private static readonly Structure Empty = new();

    public static void Init()
    {
        // *primarily* for map generation
        _simplex = new FastNoiseLite();
        _simplex.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        _simplex.SetSeed(Game.Seed);
        _simplex.SetFrequency(0.05f);
        _simplex.SetFractalType(FastNoiseLite.FractalType.FBm);
        _simplex.SetFractalOctaves(2);

        // for structures spawning, and anything that needs white noise
        _value = new FastNoiseLite();
        _value.SetNoiseType(FastNoiseLite.NoiseType.Value);
        _value.SetSeed(Game.Seed);
        _value.SetFrequency(10);

        // for decoration spawning likeliness
        _value2 = new FastNoiseLite();
        _value2.SetNoiseType(FastNoiseLite.NoiseType.Value);
        _value2.SetSeed(Game.Seed + 1);
        _value2.SetFrequency(0.1f);
    }

    private static float GetHeight(int x, int z)
    {
        switch (Game.Player.Level)
        {
            case 0: return _simplex.GetNoise(x, z) * 2 + 5;
            default: return -1; // method should stay private so no error can be thrown
        }
    }

    public static IEnumerable<int> GetColumn(int x, int z)
    {
        int levelY = Game.Player.Level << 7;
        float deco = _value2.GetNoise(x, levelY, z) / 2 + 0.5f; // does decoration spawn?
        float decoType = Value(x, levelY, z) / 2 + 0.5f; // in this case, which type?

        switch (Game.Player.Level)
        {
            case 0:
                int? decoration = null;
                decoType *= deco * deco;
                if (decoType < 0.001f && Value(x, -100, z) < 0.5f)
                {
                    decoType *= 1000;
                    decoration = decoType < 0.5f ? Game.Blocks.DeadPlant : Game.Blocks.DeadBush;
                }

                for (int y = 0; y < Chunk.Size; y++)
                {
                    float height = GetHeight(x, z);
                    if (y == 0) yield return Game.Blocks.Bedrock;
                    else if (y - 1 <= height && y > height && decoration != null) yield return (int)decoration;
                    else if (y > height) yield return Game.Blocks.Air;
                    else if (y + 1 > height) yield return Game.Blocks.Sand;
                    else if (y + 3 < height) yield return Game.Blocks.Sandstone;
                    else yield return Game.Blocks.RedSand;
                }

                break;
                default: throw new GenerationException("Incorrect level: " + Game.Player.Level);
        }
    }

    public static float Value(int x, int y, int z)
    {
        return _value.GetNoise(x, y, z);
    }

    public static (int, Structure) GetStruct(int x, int z)
    {
        // if a structure spawns in this column,
        // returns (spawn height, structure), otherwise (-1, empty structure)

        float p = Value(x, Game.Player.Level << 7, z) / 2 + 0.5f;

        Structure s = Empty;
        switch (Game.Player.Level)
        {
            case 0:
                break;
            case 1:
                if (p < 0.01f)
                {
                    float type = _simplex.GetNoise(x, Game.Player.Level << 7, z) / 2 + 0.5f;
                    s = Game.Structs[type < 0.15f ? "Trunk" : type < 0.4f ? "Tree" : "Bush"];
                }
                break;
        }

        if (s == Empty) return (-1, s);
        int y = (int)GetHeight(x + s.Offset.x, z + s.Offset.z);
        return (y + s.Offset.y, s);
    }
}
