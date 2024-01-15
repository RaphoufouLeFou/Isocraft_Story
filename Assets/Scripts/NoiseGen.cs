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
        _noise.SetSeed(Game.Seed);
    }
    
    public static IEnumerable<int> GetColumn(Vector3 pos)
    {
        // pos should be (x, 0 (ignored), z)
        if (Game.Level == 0) // overWorld (temporary test)
        {
            float height = _noise.GetNoise(pos.x, pos.z) * 2 + 5 +
                           _noise.GetNoise(pos.x * 10 + 1000, pos.z * 10 + 1000) / 2;
            for (int y = 0; y < Chunk.ChunkSize; y++)
            {
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
}
