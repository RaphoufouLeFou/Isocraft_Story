using System;
using System.Collections.Generic;

public static class NoiseGen
{
    private static FastNoiseLite _noise;

    public static void Init()
    {
        _noise = new FastNoiseLite();
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _noise.SetSeed((int)Game.Seed);
    }

    private static float GetHeight(int x, int z)
    {
        return _noise.GetNoise(x, z) * 2 + 7 + _noise.GetNoise(x * 10 + 1000, z * 10 + 1000) / 2;
    }

    public static IEnumerable<int> GetColumn(int x, int z)
    {
        if (Game.Level == 0) // overworld (temporary test)
        {
            for (int y = 0; y < Chunk.Size; y++)
            {
                float height = GetHeight(x, z);
                if (y == 0) yield return Game.Blocks.Bedrock;
                else if (y > height) yield return Game.Blocks.Air;
                else if (y + 1 > height) yield return Game.Blocks.Sand;
                else if (y + 2 < height) yield return Game.Blocks.Sandstone;
                else yield return Game.Blocks.RedSand;
            }

            yield break;
        }

        throw new ArgumentException("Incorrect level: " + Game.Level);
    }

    private static int Mod(int a, int b)
    {
        return (a % b + b) % b;
    }

    private static int _randA = 8765179, _randB = 3579547, _randC = 2468273;
    private static int Prng(int seed)
    {
        return Mod(_randA + _randB * seed, _randC);
    }

    private static float PrngPos(int x, int z)
    {
        return Prng(Game.Seed + Prng(31 * x ^ 37 * z)) / (float)_randC;
    }

    public static (int, Structure) GetStruct(int x, int z)
    {
        // if structure in this column, return it, otherwise null

        // get prng based on position, but not tileable
        float p = PrngPos(x, z);

        if (p < 0.005)
        {
            int y = (int)GetHeight(x, z) + 1;
            return (y, Structures.Structs["Tree"]);
        }
        
        return (-1, new Structure());
    }
}
