using System;
using System.Collections.Generic;
using System.IO;

public class Structure
{
    public readonly int X, Y, Z;
    private readonly int[,,] _blocks;
    public readonly (int x, int y, int z) Offset;
    private string _type;

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

    public Structure(string type)
    {
        _type = type;
        try
        {
            StreamReader file = new StreamReader("Assets/Structures/" + type + ".txt");
            // first line: size (x.y.z)
            string[] coords = GetDataLine(file);
            X = int.Parse(coords[0]);
            Y = int.Parse(coords[1]);
            Z = int.Parse(coords[2]);
            _blocks = new int[X, Y, Z];

            // second line: origin for height offset: x/z=origin, y=offset (x.y.z)
            string[] origin = GetDataLine(file);
            Offset = (int.Parse(origin[0]), int.Parse(origin[1]), int.Parse(origin[2]));
            
            // third line: blocks (x, then z, then y, separated by .)
            string[] data = GetDataLine(file);
            if (data.Length != X * Y * Z) throw new Exception();
            for (int i = 0; i < data.Length; i++)
            {
                int b = data[i] == "" ? -1 : int.Parse(data[i]); // empty value: -1 (ignore)
                _blocks[i % X, i / X / Z, i / X % Z] = b;
            }
        }
        catch
        {
            throw new ArgumentException("Error loading structure \"" + type + "\"");
        }
    }
    
    public int GetBlock(int x, int y, int z, int dx, int dy, int dz)
    {
        // used to add additional generation conditions to structures
        if (_type == "Tree")
        {
            if ((dx, dy, dz) == (1, 0, 1)) return Game.Blocks.RedSand;
            if (dy == 5)
            {
                int d = Math.Abs(dx - 1) + Math.Abs(dz - 1);
                float p = d == 1 ? 0.8f : d == 2 ? 0.4f : 1;
                return NoiseGen.PrngPos(x + dx, y + dy, z + dz) < p ? Game.Blocks.OakLeaves : Game.Blocks.Air;
            }
        }
        return _blocks[dx, dy, dz];
    }
}

public static class Structures
{
    public static readonly Dictionary<string, Structure> Structs = new();
    private static readonly string[] Names = { "Tree" };
    [NonSerialized] public static int MaxSize; // how many blocks out can structures be searched for

    private static bool _init; // static init

    public static void Init()
    {
        if (_init) return;
        _init = true;
        foreach (string type in Names)
        {
            Structure s = new Structure(type);
            MaxSize = MaxSize > s.X ? MaxSize > s.Z ? MaxSize : s.Z : s.X > s.Z ? s.X : s.Z;
            Structs.Add(type, s);
        }
    }
}