using System;
using System.Collections.Generic;
using System.IO;

public class Structure
{
    public readonly int X, Y, Z;
    public readonly int[,,] Blocks;
    public readonly (int x, int y, int z) Offset;

    private string[] GetDataLine(StreamReader file)
    {
        string line = file.ReadLine();
        if (line == null) throw new Exception();
        return line.Split(".");
    }

    public Structure()
    {
        // dummy constructor, no data
    }

    public Structure(string path)
    {
        try
        {
            StreamReader file = new StreamReader(path);
            // first line: size (x.y.z)
            string[] coords = GetDataLine(file);
            X = int.Parse(coords[0]);
            Y = int.Parse(coords[1]);
            Z = int.Parse(coords[2]);
            Blocks = new int[X, Y, Z];

            // second line: origin for height offset: x/z=origin, y=offset (x.y.z)
            string[] origin = GetDataLine(file);
            Offset = (int.Parse(origin[0]), int.Parse(origin[1]), int.Parse(origin[2]));
            
            // third line: blocks (x, then z, then y, separated by .)
            string[] data = GetDataLine(file);
            if (data.Length != X * Y * Z) throw new Exception();
            for (int i = 0; i < data.Length; i++)
            {
                int b = data[i] == "" ? -1 : int.Parse(data[i]); // empty value: -1 (ignore)
                Blocks[i % X, i / X / Z, i / X % Z] = b;
            }
        }
        catch
        {
            throw new ArgumentException("Error loading structure file: " + path);
        }
    }
}

public static class Structures
{
    public static readonly Dictionary<string, Structure> Structs = new();
    private static readonly string[] Names = { "Tree" };
    [NonSerialized] public static int MaxSize; // how many blocks out can structures be searched for

    public static void Init()
    {
        foreach (string name in Names)
        {
            Structure s = new Structure("Assets/Structures/" + name + ".txt");
            MaxSize = MaxSize > s.X ? MaxSize > s.Z ? MaxSize : s.Z : s.X > s.Z ? s.X : s.Z;
            Structs.Add(name, s);
        }
    }
}