using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : NetworkBehaviour
{
    public new Camera camera;
    public PlayerCamera playerCamera;

    [FormerlySerializedAs("NetManager")] public NetworkManagement netManager;

    [NonSerialized] public CustomRigidBody Body;
    [NonSerialized] public float GroundedHeight; // height at which the player was last grounded
    [NonSerialized] public Vector3 Spawn;

    public Inventory Inventory;
    
    [FormerlySerializedAs("Sprites")] public Sprite[] sprites;

    void Start()
    {
        //camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        GameObject items = GameObject.Find("HotBarBackground");
        for (int i = 0; i < 9; i++)
        {
            Hotbar.ItemImages[i] = items.transform.GetChild(i).gameObject;
        }
        Inventory = new Inventory();
        camera = GetComponentInChildren<Camera>();
        if (!isLocalPlayer) { 
            camera.enabled = false;
            return;
        }
        camera.enabled = true;

        netManager = GameObject.Find("NetworkManager").GetComponent<NetworkManagement>();

        Debug.Log("Transform Tag is: " + camera.gameObject.tag);
        Transform tr = transform;
        Body = new CustomRigidBody(tr, 8, 0.9f, 1.3f, -5, 0.95f, 1.85f);
        SetSpawn(0, 0);
        tr.position = Spawn;
    }

    void SetSpawn(int x, int z)
    {
        Spawn = new Vector3(x + 0.5f, Chunk.Size1, z + 0.5f);
        int chunkX = x / Chunk.Size, chunkZ = z / Chunk.Size;
        if (MapHandler.Chunks.TryGetValue(chunkX + "." + chunkZ, out Chunk chunk))
        {
            int modX = (x - chunkX * Chunk.Size) * Chunk.Size * Chunk.Size,
                modZ = z - chunkZ * Chunk.Size;
            while (chunk.Blocks[modX, (int)Spawn.y, modZ] == 0) Spawn.y--;
        }
        else throw new ArgumentException("Cannot set spawn in unloaded chunk");

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


    [ClientRpc]
    void ServerPlaceBreak(Vector3 pos, int type, bool isPlacing)
    {
        PlaceBreak(pos, type, isPlacing);
    }

    [Command]
    void ClientPlaceBreak(Vector3 pos, int type, bool isPlacing)
    {
        PlaceBreak(pos, type, isPlacing);
    }

    int PlaceBreak(Vector3 pos, int type, bool isPlacing)
    {
        int chunkX = Floor(pos.x / Chunk.Size),
            chunkZ = Floor(pos.z / Chunk.Size);
        Chunk chunk = MapHandler.Chunks[chunkX + "." + chunkZ];

        int x = Floor(pos.x) - chunkX * Chunk.Size,
            y = Floor(pos.y),
            z = Floor(pos.z) - chunkZ * Chunk.Size;

        int result = type; // for inventory management
        if (y < 0 || y >= Chunk.Size) return -1; // outside of world height
        if (isPlacing) chunk.Blocks[x, y, z] = type;
        else
        {
            result = chunk.Blocks[x, y, z];
            chunk.Blocks[x, y, z] = 0;
        }
        chunk.BuildMesh();

        // update nearby chunks if placed on a chunk border
        List<string> toCheck = new();
        if (x == 0) toCheck.Add(chunkX - 1 + "." + chunkZ);
        else if (x == Chunk.Size1) toCheck.Add(chunkX + 1 + "." + chunkZ);
        if (z == 0) toCheck.Add(chunkX + "." + (chunkZ - 1));
        else if (z == Chunk.Size1) toCheck.Add(chunkX + "." + (chunkZ + 1));
        foreach (string chunkName in toCheck)
            if (MapHandler.Chunks.ContainsKey(chunkName))
                MapHandler.Chunks[chunkName].BuildMesh();
        return result;
    }

    void DetectPlaceBreak()
    {
        bool left = Input.GetMouseButtonDown(0), right = Input.GetMouseButtonDown(1);
        if (left || right)
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // move into or out of the block to get the right targeted block
                hit.point += 0.01f * (right ? 1 : -1) * hit.normal;

                int currentBlock = Inventory.GetCurrentBlock(Hotbar.SelectedIndex, 3);
                if (right)
                {
                    int count = Inventory.GetCurrentBlockCount(Hotbar.SelectedIndex, 3);
                    if (count <= 0) return;
                    int res = PlaceBreak(hit.point, currentBlock, true); // place the block for this instance
                    if (res != -1) Inventory.RemoveBlock(Hotbar.SelectedIndex, 3, sprites[0]);
                }
                else
                {
                    int res = PlaceBreak(hit.point, currentBlock, false); // place the block for this instance
                    if (res != -1) Inventory.AddBlock(res, sprites[res]);
                }

                if (isServer) ServerPlaceBreak(hit.point, currentBlock, right); // server tells clients to place the block
                else ClientPlaceBreak(hit.point, currentBlock, right); // client tells the server to place the block
            }
        }
    }
    
    void Update()
    {
        if (!isLocalPlayer) return;

        Hotbar.UpdateHotBar(); 

        Body.Update(netManager.IsPaused);

        if (Body.OnFloor) GroundedHeight = transform.position.y;

        if (netManager.IsPaused) return;
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

        DetectPlaceBreak();

        if (Input.GetKeyDown(KeyCode.K)) // kill
        {
            transform.position = Spawn;
            Body.Movement = Vector3.zero;
        }

        Vector3 pos = transform.position;
        if (Input.GetKeyDown(KeyCode.R)) SetSpawn((int)pos.x, (int)pos.z); // set spawn
    }
}
