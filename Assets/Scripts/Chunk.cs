using System;
using System.Collections.Generic;
using UnityEngine;

class FaceUtils
{
    // front, back, top, bottom, right, left
    private readonly int[,] _facesIndices =
        { { 5, 7, 6, 4 }, { 0, 2, 3, 1 }, { 2, 6, 7, 3 }, { 4, 0, 1, 5 }, { 1, 3, 7, 5 }, { 4, 6, 2, 0 } };
    public readonly List<Vector3[]> FacesOffsets = new();

    public FaceUtils()
    {
        // initialize vertex lists
        List<Vector3> vertexOffset = new List<Vector3>();
        for (int i = 0; i < 8; i++) vertexOffset.Add(new Vector3(i & 1, i >> 1 & 1, i >> 2));
        for (int face = 0; face < 6; face++)
            FacesOffsets.Add(new[]
            {
                vertexOffset[_facesIndices[face, 0]],
                vertexOffset[_facesIndices[face, 1]],
                vertexOffset[_facesIndices[face, 2]],
                vertexOffset[_facesIndices[face, 3]]
            });
    }
}

public class Chunk : MonoBehaviour
{
    [NonSerialized] public const int ChunkSize = 8;
    [NonSerialized] public const int Size1 = ChunkSize - 1;
    private Vector2 _pos;
    [NonSerialized] public int[,,] Blocks = new int[ChunkSize, ChunkSize, ChunkSize];

    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;
    private readonly FaceUtils _faceUtils = new();

    public void Init(Vector3 pos)
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        _pos = new Vector2(pos.x, pos.z);
        transform.position = pos * ChunkSize;
        GenerateBlocks();
        BuildMesh(true);
    }
    
    void GenerateBlocks()
    {
        for (int x = 0; x < ChunkSize; x++)
        for (int z = 0; z < ChunkSize; z++)
        {
            int y = 0;
            foreach (int block in NoiseGen.GetColumn(transform.position + new Vector3(x, 0, z)))
                Blocks[x, y++, z] = block;
        }
    }

    public void BuildMesh(bool newChunk = false)
    {
        // returns a list of chunks that need to be updated

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int nFaces = 0;
        
        // get neighbors
        Dictionary<int, Chunk> neighbors = new ();
        for (int i = 0; i < 4; i++)
        {
            if (MapHandler.Chunks.TryGetValue(_pos.x + (i < 2 ? i * 2 - 1 : 0) + "." +
                                              (_pos.y + (i > 1 ? i * 2 - 5 : 0)), out Chunk chunk))
                neighbors.Add(i, chunk);
        }

        for (int x = 0; x < ChunkSize; x++)
            for (int y = 0; y < ChunkSize; y++)
                for (int z = 0; z < ChunkSize; z++)
                {
                    if (Blocks[x, y, z] == 0) continue;

                    Vector3 pos = new Vector3(x, y, z);
                    for (int face = 0; face < 6; face++)
                    {
                        // get other block, offset according to face
                        Vector3 otherPos = pos;
                        if (face == 0) otherPos.z++;
                        else if (face == 1) otherPos.z--;
                        else if (face == 2) otherPos.y++;
                        else if (face == 3) otherPos.y--;
                        else if (face == 4) otherPos.x++;
                        else otherPos.x--;
                        int other = -1; // other block
                        int i = 0;
                        if (otherPos.x < 0)
                        {
                            otherPos.x += ChunkSize;
                            i = 0;
                        }
                        else if (otherPos.x > Size1)
                        {
                            otherPos.x -= ChunkSize;
                            i = 1;
                        }
                        else if (otherPos.y < 0) other = 0; // air under
                        else if (otherPos.y >= ChunkSize) other = 0; // air above
                        else if (otherPos.z < 0)
                        {
                            otherPos.z += ChunkSize;
                            i = 2;
                        }
                        else if (otherPos.z > Size1)
                        {
                            otherPos.z -= ChunkSize;
                            i = 3;
                        }
                        else other = Blocks[(int)otherPos.x, (int)otherPos.y, (int)otherPos.z]; // block in the chunk

                        if (other == -1) // block in another chunk
                            other = neighbors.TryGetValue(i, out Chunk chunk)
                                ? chunk.Blocks[(int)otherPos.x, (int)otherPos.y, (int)otherPos.z] // block in neighbor chunk
                                : 0; // air in unloaded chunks
                        if (other == 0)
                        {
                            // visible face
                            for (int j = 0; j < 4; j++) vertices.Add(pos + _faceUtils.FacesOffsets[face][j]);

                            int n = nFaces << 2;
                            triangles.AddRange(new[] { n, n + 1, n + 2, n, n + 2, n + 3 });
                            uvs.AddRange(Game.Blocks.FromId[Blocks[x, y, z]].GetUVs(face));
                            nFaces++;
                        }
                    }
                }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        _meshFilter.mesh = mesh;
        _meshCollider.sharedMesh = mesh;
        
        // update neighbors (remove side faces if needed) if newly created chunk
        if (newChunk)
            foreach (Chunk c in neighbors.Values)
                c.BuildMesh();
    }
}
