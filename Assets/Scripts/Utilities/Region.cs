using UnityEngine;

public class Region
{
    // regions have a fixed size of 8x8 chunks, centered at the origin
    // e.g. region r.1.0 contains chunks from c.4.0 to c.11.7
    
    public readonly int Rx, Rz;
    
    public static Region FromChunkPos(int cx, int cz)
    {
        (int dx, int rx) = Utils.DivMod(cx - 4, 8);
        (int dz, int rz) = Utils.DivMod(cz - 4, 8);
        Debug.Log(rx + " " + rz);
        return new Region(dx, dz);
    }
    
    public Region(int rx, int rz)
    {
        Rx = rx;
        Rz = rz;
    }
}

public static class Test
{
    static Test()
    {
        Debug.Log(Region.FromChunkPos(0, 0));
        Debug.Log(Region.FromChunkPos(-3, -3));
        Debug.Log(Region.FromChunkPos(-4, -4));
        Debug.Log(Region.FromChunkPos(4, 0));
        Debug.Log(Region.FromChunkPos(11, 7));
        Debug.Log(Region.FromChunkPos(12, 7));
        Debug.Log(Region.FromChunkPos(11, 8));
        Debug.Log(Region.FromChunkPos(12, 8));
    }
}