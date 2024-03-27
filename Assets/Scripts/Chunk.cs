using System;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    [NonSerialized] public const int Size = 16;
    [NonSerialized] public const int Size1 = Size - 1;
    [NonSerialized] public int[,,] Blocks;
    private readonly Dictionary<(int x, int y, int z), IBlockEntity> _models = new();

    private int _x, _z, _cx, _cz; // in chunk space
    public MeshFilter opaqueMeshFilter, transparentMeshFilter;
    public MeshCollider meshCollider;

    public static string GetName(int cx, int cz)
    {
        return $"c.{cx}.{cz}";
    }

    public void Init(int cx, int cz, int[,,] blocks)
    {
        _x = cx * Size;
        _z = cz * Size;
        _cx = cx;
        _cz = cz;
        name = GetName(cx, cz);
        Blocks = blocks;

        transform.position = new Vector3(_x, 0, _z);

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
            if (y != -1) for (int dx = 0; dx < s.X; dx++)
                for (int dy = 0; dy < s.Y; dy++) for (int dz = 0; dz < s.Z; dz++)
                    if (x + dx is >= 0 and < Size && y + dy is >= 0 and < Size && z + dz is >= 0 and < Size)
                    {
                        int b = s.GetBlock(x, y, z, dx, dy, dz);
                        if (b != -1) blocks[x + dx, y + dy, z + dz] = b;
                    }
        }
    }

    public bool InteractBlock(int x, int y, int z)
    {
        if (!_models.TryGetValue((x, y, z), out IBlockEntity blockEntity)) return false;

        blockEntity.Interact();
        return true;
    }

    public void RemoveEntity(int x, int y, int z)
    {
        if (!_models.TryGetValue((x, y, z), out IBlockEntity blockEntity)) return;

        Destroy(blockEntity.GameObject);
        _models.Remove((x, y, z));
    }

    public void BuildMesh(bool newChunk = false)
    {
        // build mesh from blocks
        // update the neighboring chunks that need to be updated

        Mesh mesh1 = new(), mesh2 = new(), colMesh = new();
        List<Vector3> vertices1 = new(), vertices2 = new(), colVertices = new();
        List<int> triangles1 = new(), triangles2 = new(), colTriangles = new();
        List<Vector2> uvs1 = new(), uvs2 = new();
        int n1 = 0, n2 = 0, n3 = 0;

        // get neighboring chunks
        Dictionary<int, Chunk> neighbors = new ();
        for (int i = 0; i < 4; i++)
            if (MapHandler.Chunks.TryGetValue(GetName(_cx + (i < 2 ? i * 2 - 1 : 0), _cz + (i > 1 ? i * 2 - 5 : 0)),
                    out Chunk chunk))
                neighbors.Add(i, chunk);

        for (int x = 0; x < Size; x++) for (int y = 0; y < Size; y++)
        for (int z = 0; z < Size; z++)
        {
            int blockId = Blocks[x, y, z];
            Block blockObj = Game.Blocks.FromId[blockId];
            Vector3 pos = new Vector3(x, y, z);

            if (blockObj.IsModel) // 3D model: add model to children
            {
                IBlockEntity blockEntity = BlockEntity.Create(blockId);
                GameObject go = blockEntity.GetBaseObject();
                go = Instantiate(go, transform);
                blockEntity.SetObject(go, new Vector3(_x + x, y, _z + z));
                _models[(x, y, z)] = blockEntity;
            }

            else if (blockObj.Is2D) // cross-shaped block: display all cross faces
            {
                for (int face = 0; face < 4; face++)
                {
                    for (int j = 0; j < 4; j++) vertices2.Add(pos + FaceUtils.CrossOffsets[face][j]);

                    triangles2.AddRange(new[] { n2, n2 + 1, n2 + 2, n2, n2 + 2, n2 + 3 });
                    uvs2.AddRange(blockObj.GetUVs(face));
                    n2 += 4;
                }
            }

            // full block: display face by face under certain conditions
            // collide block: still need to execute all of this to build the collider
            // only except 2D blocks which are handled above
            if (!blockObj.Is2D && !blockObj.NoTexture || !blockObj.NoRayCast)
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

                    int otherId = -1; // other block
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

                    else if (otherPos.y < 0) otherId = 0; // air under
                    else if (otherPos.y >= Size) otherId = 0; // air above
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
                        otherId = Blocks[(int)otherPos.x, (int)otherPos.y,
                            (int)otherPos.z]; // other block is in the same chunk

                    if (otherId == -1) // other block is in another chunk
                        otherId = neighbors.TryGetValue(i, out Chunk chunk)
                            ? chunk.Blocks
                                [(int)otherPos.x, (int)otherPos.y, (int)otherPos.z] // block is in neighboring chunk
                            : Game.Blocks.Bedrock; // block in unloaded chunk: avoid rendering useless faces
                    Block otherObj = Game.Blocks.FromId[otherId];

                    if (!blockObj.Is2D && !blockObj.NoTexture)
                    {
                        // draw solid blocks faces next to transparent blocks
                        if (!blockObj.Transparent && otherObj.Transparent)
                        {
                            for (int j = 0; j < 4; j++) vertices1.Add(pos + FaceUtils.FacesOffsets[face][j]);

                            triangles1.AddRange(new[] { n1, n1 + 1, n1 + 2, n1, n1 + 2, n1 + 3 });
                            uvs1.AddRange(blockObj.GetUVs(face));
                            n1 += 4;
                        }

                        // handle displaying faces of transparent blocks
                        else if (blockObj.Transparent &&
                                 (otherObj.Transparent && !Settings.Game.FastGraphics || otherObj.NoTexture) &&
                                 !(blockObj.IsFluid && otherObj.IsFluid))
                        {
                            for (int j = 0; j < 4; j++) vertices2.Add(pos + FaceUtils.FacesOffsets[face][j]);

                            triangles2.AddRange(new[] { n2, n2 + 1, n2 + 2, n2, n2 + 2, n2 + 3 });
                            uvs2.AddRange(blockObj.GetUVs(face));
                            n2 += 4;
                        }
                    }

                    // handle RayCast collider
                    if (!blockObj.NoRayCast && otherObj.NoRayCast)
                    {
                        for (int j = 0; j < 4; j++) colVertices.Add(pos + FaceUtils.FacesOffsets[face][j]);

                        colTriangles.AddRange(new[] { n3, n3 + 1, n3 + 2, n3, n3 + 2, n3 + 3 });
                        n3 += 4;
                    }
                }
            }
        }

        mesh1.vertices = vertices1.ToArray();
        mesh1.triangles = triangles1.ToArray();
        mesh1.uv = uvs1.ToArray();
        mesh1.RecalculateNormals();
        opaqueMeshFilter.mesh = mesh1;

        mesh2.vertices = vertices2.ToArray();
        mesh2.triangles = triangles2.ToArray();
        mesh2.uv = uvs2.ToArray();
        mesh2.RecalculateNormals();
        transparentMeshFilter.mesh = mesh2;

        colMesh.vertices = colVertices.ToArray();
        colMesh.triangles = colTriangles.ToArray();
        colMesh.RecalculateNormals();
        meshCollider.sharedMesh = colMesh;

        // add to mapHandler, and update neighbors (remove side faces if needed) if newly created chunk
        if (newChunk)
        {
            MapHandler.Chunks.TryAdd(name, this);
            foreach (Chunk c in neighbors.Values)
                c.BuildMesh();
        }
    }
}
