using UnityEngine;

public class Region
{
    // regions have a fixed size of 8x8 chunks, centered at the origin
    // e.g. region r.1.0 contains chunks from c.4.0 to c.11.7
    
    private readonly int _rx, _rz;
    
    public static Region FromChunkPos(int cx, int cz)
    {
        return new Region(Utils.Floor((cx + 4) / 8f), Utils.Floor((cz + 4) / 8f));
    }

    public string GetName()
    {
        return $"r.{_rx}.{_rz}";
    }
    
    private Region(int rx, int rz)
    {
        _rx = rx;
        _rz = rz;
    }
}