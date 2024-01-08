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
    [NonSerialized] public const int ChunkSize = 16;
    [NonSerialized] private const int ChunkSize2 = ChunkSize * ChunkSize;
    private const int Size1 = ChunkSize - 1;
    private Vector2 _pos;

    [NonSerialized] public List<int> Blocks;

    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;
    private readonly FaceUtils _faceUtils = new();
    private readonly int[] _offsetIndex = { 1, -1, ChunkSize, -ChunkSize, ChunkSize2, -ChunkSize2 };

    public void Init(Vector3 pos)
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        _pos = new Vector2(pos.x, pos.z);
        transform.position = pos * ChunkSize;
        GenerateBlocks();
        BuildMesh();
    }
    
    void GenerateBlocks()
    {
        Blocks = new List<int>();
        for (int x = 0; x < ChunkSize; x++)
            for (int y = 0; y < ChunkSize; y++)
            for (int z = 0; z < ChunkSize; z++)
                Blocks.Add(NoiseGen.GetBlock(transform.position + new Vector3(x, y, z)));
    }

    public void BuildMesh()
    {
        Vector3 chunkPos = transform.position;
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
                    int i = (x * ChunkSize + y) * ChunkSize + z;
                    if (Blocks[i] == 0) continue;

                    Vector3 pos = new Vector3(x, y, z);
                    for (int face = 0; face < 6; face++)
                    {
                        int i2 = i + _offsetIndex[face];
                        // handle blocks outside of chunks
                        Vector3 otherPos = pos;
                        int other = -1;
                        if (face == 0 && z == Size1) otherPos.z += 1;
                        else if (face == 1 && z == 0) otherPos.z -= 1;
                        else if (face == 2 && y == Size1) other = 0; // air on top
                        else if (face == 3 && y == 0) other = 0; // air under
                        else if (face == 4 && x == Size1) otherPos.x += 1;
                        else if (face == 5 && x == 0) otherPos.x -= 1;
                        else other = Blocks[i2];
                        if (other == -1) // block out of chunk
                        {
                            // get block in generated chunk
                            int j;
                            Vector3 wrapped = otherPos; // other pos, once wrapped in its chunk
                            if (wrapped.x < 0)
                            {
                                wrapped.x += ChunkSize;
                                j = 0;
                            }
                            else if (wrapped.x > Size1)
                            {
                                wrapped.x -= ChunkSize;
                                j = 1;
                            }
                            else if (wrapped.z < 0)
                            {
                                wrapped.z += ChunkSize;
                                j = 2;
                            }
                            else
                            {
                                wrapped.z -= ChunkSize;
                                j = 3;
                            }

                            other = neighbors.TryGetValue(j, out Chunk chunk)
                                ? chunk.Blocks[(int)((wrapped.x * ChunkSize + wrapped.y) * ChunkSize + wrapped.z)]
                                : NoiseGen.GetBlock(chunkPos + otherPos);
                        }
                        if (other == 0)
                        {
                            // visible face
                            for (int j = 0; j < 4; j++) vertices.Add(pos + _faceUtils.FacesOffsets[face][j]);

                            int n = nFaces << 2;
                            triangles.AddRange(new[] { n, n + 1, n + 2, n, n + 2, n + 3 });
                            uvs.AddRange(Game.Blocks.FromId[Blocks[i]].GetUVs(face));
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
    }
}
