using System;
using System.Collections.Generic;
using UnityEngine;

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

    public static (Vector3, int) GetStruct(int x, int z)
    {
        // if structure in this column, returns (origin, type), otherwise ((0, 0, 0), -1)
        int type = (5 * x + 3 * z) % 10;
        if (type < 5) return (new Vector3(x, (int)GetHeight(x, z), z), type);
        return (new Vector3(), -1);
    }
}
