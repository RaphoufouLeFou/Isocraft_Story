using System;
using System.Collections.Generic;
using System.IO;

public class Structure
{
    public readonly int X, Y, Z;
    public readonly int[,,] Blocks;

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
            string[] coords = GetDataLine(file);
            X = int.Parse(coords[0]);
            Y = int.Parse(coords[1]);
            Z = int.Parse(coords[2]);
            Blocks = new int[X, Y, Z];

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
    public static Dictionary<string, Structure> Structs = new();
    private static readonly string[] Names = { "Trunk", "Tree" };
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