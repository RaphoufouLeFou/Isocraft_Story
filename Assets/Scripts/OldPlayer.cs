using System;
using System.Collections.Generic;
using UnityEngine;

public class OldPlayer : MonoBehaviour
{
    public new Camera camera;
    public PlayerCamera playerCamera;

    [NonSerialized] public CustomRigidBody Body;
    [NonSerialized] public float GroundedHeight; // height at which the player was last grounded
    [NonSerialized] public Vector3 Spawn;

    void Start()
    {
        Transform tr = transform;
        Body = new CustomRigidBody(tr, 8, 0.9f, 1.3f, -5, 0.95f, 1.85f);
        //SetSpawn(0, 0);
        tr.position = Spawn;
    }

    void SetSpawn(int x, int z)
    {
        Spawn = new Vector3(x + 0.5f, Chunk.Size1, z + 0.5f);
        int chunkX = x / Chunk.ChunkSize, chunkZ = z / Chunk.ChunkSize;
        if (MapHandler.Chunks.TryGetValue(chunkX + "." + chunkZ, out Chunk chunk))
        {
            int modX = (x - chunkX * Chunk.ChunkSize) * Chunk.ChunkSize * Chunk.ChunkSize,
                modZ = z - chunkZ * Chunk.ChunkSize;
            while (chunk.Blocks[modX, (int)Spawn.y, modZ] == 0) Spawn.y--;
        }
        else
            throw new ArgumentException("Cannot set spawn in unloaded chunk");

        Spawn.y++;
    }
    
    int Floor(float x)
    {
        if (x < 0)
        {
            int offset = 1 - (int)x;
            return (int)(x + offset) - offset;
        }
        return (int)x;
    }
    
    void PlaceBreak()
    {
        bool left = Input.GetMouseButtonDown(0), right = Input.GetMouseButtonDown(1);
        if (left || right)
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // move into or out of the block to get the right targeted block
                hit.point += 0.01f * (right ? 1 : -1) * hit.normal;

                int chunkX = Floor(hit.point.x / Chunk.ChunkSize),
                    chunkZ = Floor(hit.point.z / Chunk.ChunkSize);
                Chunk chunk = MapHandler.Chunks[chunkX + "." + chunkZ];

                int x = Floor(hit.point.x) - chunkX * Chunk.ChunkSize,
                    y = Floor(hit.point.y),
                    z = Floor(hit.point.z) - chunkZ * Chunk.ChunkSize;

                if (y < 0 || y >= Chunk.ChunkSize) return; // outside of world height
                if (left) chunk.Blocks[x, y, z] = 0;
                else chunk.Blocks[x, y, z] = 5;
                chunk.BuildMesh();
                
                // update nearby chunks if placed on a chunk border
                List<string> toCheck = new ();
                if (x == 0) toCheck.Add(chunkX - 1 + "." + chunkZ);
                else if (x == Chunk.ChunkSize - 1) toCheck.Add(chunkX + 1 + "." + chunkZ);
                if (z == 0) toCheck.Add(chunkX + "." + (chunkZ - 1));
                else if (z == Chunk.ChunkSize - 1) toCheck.Add(chunkX + "." + (chunkZ + 1));
                foreach (string chunkName in toCheck)
                    if (MapHandler.Chunks.ContainsKey(chunkName))
                        MapHandler.Chunks[chunkName].BuildMesh();
            }
        }
    }
    
    void Update()
    {
        Body.Update(false);
        if (Body.OnFloor) GroundedHeight = transform.position.y;

        // rotate camera about the Y axis
        Vector3 rotation = transform.rotation.eulerAngles;
        bool change = false;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            change = true;
            rotation.y -= 45;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            change = true;
            rotation.y += 45;
        }
        if (change) playerCamera.GoalRot.y = rotation.y;

        rotation.y = camera.transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(rotation);

        PlaceBreak();

        if (Input.GetKeyDown(KeyCode.K)) // kill
        {
            transform.position = Spawn;
            Body.Movement = Vector3.zero;
        }

        Vector3 pos = transform.position;
        if (Input.GetKeyDown(KeyCode.R)) SetSpawn((int)pos.x, (int)pos.z); // set spawn
    }
}
