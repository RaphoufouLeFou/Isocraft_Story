using System;
using System.Collections.Generic;

public static class NoiseGen
{
    private static FastNoiseLite _simplex;
    private static FastNoiseLite _value;
    private static readonly Structure Empty = new();

    public static void Init()
    {
        _simplex = new FastNoiseLite();
        _simplex.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        _simplex.SetSeed(Game.Seed);
        _simplex.SetFrequency(0.05f);
        _simplex.SetFractalType(FastNoiseLite.FractalType.FBm);
        _simplex.SetFractalOctaves(2);

        _value = new FastNoiseLite();
        _value.SetNoiseType(FastNoiseLite.NoiseType.Value);
        _value.SetSeed(Game.Seed);
        _value.SetFrequency(10);
    }

    private static float GetHeight(int x, int z)
    {
        switch (Game.Player.Level)
        {
            case 0: return _simplex.GetNoise(x, z) * 2 + 5;
            default: throw new ArgumentException("Incorrect level: " + Game.Player.Level);
        }
    }

    public static IEnumerable<int> GetColumn(int x, int z)
    {
        if (Game.Player.Level == 0)
        {
            for (int y = 0; y < Chunk.Size; y++)
            {
                float height = GetHeight(x, z);
                if (y == 0) yield return Game.Blocks.Bedrock;
                else if (y > height) yield return Game.Blocks.Air;
                else if (y + 1 > height) yield return Game.Blocks.Sand;
                else if (y + 3 < height) yield return Game.Blocks.Sandstone;
                else yield return Game.Blocks.RedSand;
            }
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

        float p = _value.GetNoise(x, z) / 2 + 0.5f;

        if (p < 0.01)
        {
            float type = _simplex.GetNoise(x, z) / 2 + 0.5f;
            Structure s = Game.Structs[type < 0.15 ? "Trunk" : type < 0.4 ? "Tree" : "Bush"];
            int y = (int)GetHeight(x + s.Offset.x, z + s.Offset.z);
            return (y + s.Offset.y, s);
        }
        
        return (-1, Empty);
    }
}
