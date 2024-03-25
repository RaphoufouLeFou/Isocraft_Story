using System;
using System.Collections.Generic;
using UnityEngine;

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
        
        List<Vector3> crossVertexOffset = new List<Vector3>();
        for (int i = 0; i < 8; i++) crossVertexOffset.Add(new Vector3(i & 1, i >> 1 & 1, i >> 2));
        // TODO
        
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

public class Chunk : MonoBehaviour
{
    [NonSerialized] public const int Size = 16;
    [NonSerialized] public const int Size1 = Size - 1;
    [NonSerialized] public int[,,] Blocks;

    private int _cx, _cz; // in chunk space 
    private MeshFilter _meshFilter1, _meshFilter2;
    private MeshCollider _collider;

    public void Init(int cx, int cz, int[,,] blocks)
    {
        _cx = cx;
        _cz = cz;
        transform.position = new Vector3(cx * Size, 0, cz * Size);
        name = $"{cx}.{cz}";
        Blocks = blocks;

        GameObject opaquePlane = transform.Find("Opaque").gameObject;
        GameObject transparentPlane = transform.Find("Transparent").gameObject;
        
        _meshFilter1 = opaquePlane.GetComponent<MeshFilter>();
        _meshFilter2 = transparentPlane.GetComponent<MeshFilter>();
        _collider = GetComponent<MeshCollider>();
        
        BuildMesh(true);
    }
    
    public static void GenerateBlocks(int[,,] blocks, int posX, int posZ)
    {
        // generate blocks and structures from NoiseGen into an int[,,] array

        // blocks
        for (int x = 0; x < Size; x++)
        for (int z = 0; z < Size; z++)
        {
            int y = 0;
            foreach (int block in NoiseGen.GetColumn(posX + x, posZ + z))
                blocks[x, y++, z] = block;
        }

        // get intersecting structures by searching around
        for (int x = -Game.MaxStructSize; x < Size + Game.MaxStructSize; x++)
        for (int z = -Game.MaxStructSize; z < Size + Game.MaxStructSize; z++)
        {
            (int y, Structure s) = NoiseGen.GetStruct(posX + x, posZ + z);
            if (y != -1) for(int dx = 0; dx < s.X; dx++) for(int dy = 0; dy < s.Y; dy++) for (int dz = 0; dz < s.Z; dz++)
                if (x + dx is >= 0 and < Size && y + dy is >= 0 and < Size && z + dz is >= 0 and < Size)
                {
                    int b = s.GetBlock(x, y, z, dx, dy, dz);
                    if (b != -1) blocks[x + dx, y + dy, z + dz] = b;
                }
        }
    }

    public void BuildMesh(bool newChunk = false)
    {
        // build mesh from blocks
        // update the neighboring chunks that need to be updated

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int nFaces = 0;
        
        // get neighboring chunks
        Dictionary<int, Chunk> neighbors = new ();
        for (int i = 0; i < 4; i++)
            if (MapHandler.Chunks.TryGetValue($"{_cx + (i < 2 ? i * 2 - 1 : 0)}.{_cz + (i > 1 ? i * 2 - 5 : 0)}",
                    out Chunk chunk))
                neighbors.Add(i, chunk);

        for (int x = 0; x < Size; x++) for (int y = 0; y < Size; y++)
        for (int z = 0; z < Size; z++)
        {
            int blockId = Blocks[x, y, z];
            if (blockId == Game.Blocks.Air) continue;
            Block blockObj = Game.Blocks.FromId[blockId];
            Vector3 pos = new Vector3(x, y, z);

            if (blockObj is Cross) // cross-shaped block: display all cross faces
            {
                for (int face = 0; face < 4; face++)
                {
                    for (int j = 0; j < 4; j++) vertices.Add(pos + FaceUtils.CrossOffsets[face][j]);
                    
                    int n = nFaces << 2;
                    triangles.AddRange(new[] { n, n + 1, n + 2, n, n + 2, n + 3 });
                    uvs.AddRange(blockObj.GetUVs(face));
                    nFaces++;
                }
            }
            else // full block: display face by face if visible
            {
                for (int face = 0; face < 6; face++)
                {
                    // get other block, offset according to face
                    Vector3 otherPos = pos;
                    switch (face)
                    {
                        case 0:
                            otherPos.z++;
                            break;
                        case 1:
                            otherPos.z--;
                            break;
                        case 2:
                            otherPos.y++;
                            break;
                        case 3:
                            otherPos.y--;
                            break;
                        case 4:
                            otherPos.x++;
                            break;
                        default:
                            otherPos.x--;
                            break;
                    }

                    int other = -1; // other block
                    int i = 0;
                    if (otherPos.x < 0)
                    {
                        otherPos.x += Size;
                        i = 0;
                    }
                    else if (otherPos.x > Size1)
                    {
                        otherPos.x -= Size;
                        i = 1;
                    }
                    else if (otherPos.y < 0) other = 0; // air under
                    else if (otherPos.y >= Size) other = 0; // air above
                    else if (otherPos.z < 0)
                    {
                        otherPos.z += Size;
                        i = 2;
                    }
                    else if (otherPos.z > Size1)
                    {
                        otherPos.z -= Size;
                        i = 3;
                    }
                    else
                        other = Blocks[(int)otherPos.x, (int)otherPos.y,
                            (int)otherPos.z]; // other block is in the same chunk

                    if (other == -1) // other block is in another chunk
                        other = neighbors.TryGetValue(i, out Chunk chunk)
                            ? chunk.Blocks
                                [(int)otherPos.x, (int)otherPos.y, (int)otherPos.z] // block is in neighboring chunk
                            : Game.Blocks.Bedrock; // block in unloaded chunk: avoid rendering useless faces
                    if (other == Game.Blocks.Air || Game.Blocks.FromId[other] is Cross)
                    {
                        // face visible to the player
                        for (int j = 0; j < 4; j++) vertices.Add(pos + FaceUtils.FacesOffsets[face][j]);

                        int n = nFaces << 2;
                        triangles.AddRange(new[] { n, n + 1, n + 2, n, n + 2, n + 3 });
                        uvs.AddRange(blockObj.GetUVs(face));
                        nFaces++;
                    }
                }
            }
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
        _meshFilter1.mesh = mesh;
        _collider.sharedMesh = mesh;
        
        // add to mapHandler, and update neighbors (remove side faces if needed) if newly created chunk
        if (newChunk)
        {
            MapHandler.Chunks.TryAdd(name, this);
            foreach (Chunk c in neighbors.Values)
                c.BuildMesh();
        }
    }
}
