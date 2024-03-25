using System;
using System.IO;

public class Structure
{
    public readonly int X, Y, Z;
    private readonly int[,,] _blocks;
    public readonly (int x, int y, int z) Offset;
    private readonly string _type;

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
                _blocks[i / (Y * Z), i / Z % Y, i % Z] = b;
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
        int b = _blocks[dx, dy, dz];
        if (_type == "Tree")
            if (dx != 1 && dy == 5 && dz != 1 && b == -1)
                return NoiseGen.Value(x + dx, y + dy, z + dz) < 0.5f ? Game.Blocks.DesertLeaves : Game.Blocks.None;
        return b;
    }
}