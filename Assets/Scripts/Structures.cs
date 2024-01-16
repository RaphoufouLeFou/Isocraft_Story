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
                Blocks[i % X, i / X % Y, i / X / Y] = int.Parse(data[i]);
            }
        }
        catch
        {
            throw new ArgumentException("Error loading structure file: " + path);
        }
    }
}

public class Structures
{
    public Dictionary<string, Structure> Structs = new();
    private readonly string[] _names = { "Trunk", "Tree" };
    [NonSerialized] public readonly int MaxSize; // how many blocks out can structures be searched for

    public Structures()
    {
        foreach (string name in _names)
        {
            Structure s = new Structure("Assets/Structures/" + name + ".txt");
            MaxSize = MaxSize > s.X ? MaxSize > s.Z ? MaxSize : s.Z : s.X > s.Z ? s.X : s.Z;
            Structs.Add(name, s);
        }
    }
}