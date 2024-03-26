using System;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static int Mod(int a, int b)
    {
        return (a % b + b) % b;
    }

    public static (int, int) DivMod(int a, int b)
    {
        int mod = Mod(a, b);
        return ((a - mod) / b, mod);
    }

    public static int Floor(float x)
    {
        if (x < 0)
        {
            int offset = 1 - (int)x;
            return (int)(x + offset) - offset;
        }

        return (int)x;
    }

    public static float SmoothStep(float t)
    {
        return (3 - 2 * t) * t * t;
    }
}

public static class FaceUtils
{
    // for cube faces
    // front, back, top, bottom, right, left
    private static readonly int[,] FacesIndices =
        { { 5, 7, 6, 4 }, { 0, 2, 3, 1 }, { 2, 6, 7, 3 }, { 4, 0, 1, 5 }, { 1, 3, 7, 5 }, { 4, 6, 2, 0 } };
    public static readonly List<Vector3[]> FacesOffsets = new();
    
    // for cross shapes
    // front left, front right, back right, back left
    private static readonly int[,] CrossIndices =
        { { 5, 7, 2, 0 }, { 1, 3, 6, 4 }, { 0, 2, 7, 5 }, { 4, 6, 3, 1 } };
    public static readonly List<Vector3[]> CrossOffsets = new();

    static FaceUtils()
    {
        // initialize vertex lists
        List<Vector3> vertexOffset = new List<Vector3>();
        for (int i = 0; i < 8; i++) vertexOffset.Add(new Vector3(i & 1, i >> 1 & 1, i >> 2));

        float sqrt = MathF.Sqrt(2) / 4;
        List<Vector3> crossVertexOffset = new List<Vector3>();
        for (int i = 0; i < 8; i++)
            crossVertexOffset.Add(new Vector3(
                0.5f + (i % 2 * 2 - 1) * sqrt,
                i >> 1 & 1,
                0.5f + ((i >> 2) * 2 - 1) * sqrt
            ));
        
        // initialize face offsets
        for (int face = 0; face < 6; face++)
            FacesOffsets.Add(new[]
            {
                vertexOffset[FacesIndices[face, 0]],
                vertexOffset[FacesIndices[face, 1]],
                vertexOffset[FacesIndices[face, 2]],
                vertexOffset[FacesIndices[face, 3]]
            });

        for (int face = 0; face < 4; face++)
            CrossOffsets.Add(new[]
            {
                crossVertexOffset[CrossIndices[face, 0]],
                crossVertexOffset[CrossIndices[face, 1]],
                crossVertexOffset[CrossIndices[face, 2]],
                crossVertexOffset[CrossIndices[face, 3]]
            });
    }
}