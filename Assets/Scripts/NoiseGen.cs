using System;
using System.Collections.Generic;

public static class NoiseGen
{
    private static FastNoiseLite _noise;

    public static void Init()
    {
        _noise = new FastNoiseLite();
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _noise.SetSeed(Game.Seed);
    }

    private static float GetHeight(int x, int z)
    {
        return _noise.GetNoise(x, z) * 2 + 7 + _noise.GetNoise(x * 10 + 1000, z * 10 + 1000) / 2;
    }

    public static IEnumerable<int> GetColumn(int x, int z)
    {
        if (Game.Level == 0)
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

            yield break;
        }

        throw new ArgumentException("Incorrect level: " + Game.Level);
    }

    private const int A = 8765179, B = 3579547, C = 2468273;
    private static int Prng(int seed)
    {
        return Utils.Mod(A + B * seed, C); 
    }

    public static float PrngPos(int x, int y, int z)
    {
        return Prng(Game.Seed + Prng(29 * x ^ 31 * y ^ 37 * z)) / (float)C;
    }

    public static (int, Structure) GetStruct(int x, int z)
    {
        // if structure in this column, return it, otherwise null

        float p = _noise.GetNoise((long)x << 10, (long)z << 10) / 2 + 0.5f;

        if (p < 0.05)
        {
            float type = _noise.GetNoise(x << 5, z << 5) / 2 + 0.5f;
            Structure s = Game.Structs[type < 0.05 ? "Penis" : type < 0.15 ? "Trunk" : type < 0.4 ? "Tree" : "Bush"];
            int y = (int)GetHeight(x + s.Offset.x, z + s.Offset.z);
            return (y + s.Offset.y, s);
        }
        
        return (-1, new Structure());
    }
}
