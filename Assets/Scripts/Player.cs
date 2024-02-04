using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private Camera _camera;
    public PlayerCamera playerCamera;

    [NonSerialized] public CustomRigidBody Body;
    [NonSerialized] public float GroundedHeight; // height at which the player was last grounded
    private Vector3 _spawn;

    private GameObject _healthImage;
    private MapHandler _mapHandler;
    private InventoryUI _inventoryUI;

    [NonSerialized] public float Health;

    public Inventory Inventory;
    
    private void Start()
    {
        if (isLocalPlayer) Game.Player = this;
        // camera
        _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        _camera.enabled = isLocalPlayer;
        if (!isLocalPlayer) return;

        // set up other objects
        GameObject scripts = GameObject.Find("Scripts");
        _inventoryUI = scripts.GetComponent<InventoryUI>();
        _healthImage = GameObject.Find("Health bar").transform.GetChild(0).gameObject;
        _mapHandler = GameObject.Find("MapHandler").GetComponent<MapHandler>();
        
        Health = 1;
        Inventory = new();
        Inventory.AddBlock(Game.Blocks.Cobblestone, Game.InvSprites[Game.Blocks.Cobblestone], 64);
        _inventoryUI.SetPlayerInv(Inventory);
        
        HotBar.UpdateHotBarVisual(Inventory);
        
        // body settings
        Transform tr = transform;
        Body = new CustomRigidBody(tr, 8, 0.9f, 1.3f, -5, 0.95f, 1.85f);
        
        if (!SuperGlobals.StartedFromMainMenu || SuperGlobals.IsNewSave) // spawn at 0, 0 if debugging
        {
            Respawn();
            tr.position = _spawn;
        }
    }

    public void SaveLoaded(Vector3 pos, Vector3 rot, Inventory inv)
    {
        // save infos loaded: set position, rotation, inventory, respawn
        SetSpawn(pos);
        playerCamera.GoalRot.y = MathF.Round(rot.y / 45) * 45;
        Inventory = inv;
    }
    
    public void SetSpawn(Vector3 pos)
    {
        int y = pos.y < 0 ? 0 : pos.y > Chunk.Size1 ? Chunk.Size1 : (int)pos.y;

        int chunkX = Utils.Floor(pos.x / Chunk.Size), chunkZ = Utils.Floor(pos.z / Chunk.Size);
        _spawn = new Vector3(Utils.Floor(pos.x) + 0.5f, y, Utils.Floor(pos.z) + 0.5f);
        if (MapHandler.Chunks != null && MapHandler.Chunks.TryGetValue(chunkX + "." + chunkZ, out Chunk chunk))
        {
            int modX = (int)(pos.x - chunkX * Chunk.Size),
                modZ = (int)(pos.z - chunkZ * Chunk.Size);
            // try to find a height near Spawn.y
            for (int offset = 0; offset < Chunk.Size << 1; offset++)
            {
                float searchY = _spawn.y + (offset >> 1) * ((offset & 1) == 1 ? -1 : 1);
                if (searchY is >= 0 and < Chunk.Size && chunk.Blocks[modX, (int)searchY, modZ] == 0)
                {
                    _spawn.y = searchY;
                    break;
                }
            }
        }
        else throw new ArgumentException("Cannot set spawn in unloaded chunk");

        _spawn.y++; // player needs to be one block above the ground
        Respawn();
    }

    private void Respawn()
    {
        transform.position = _spawn;
        Body.Movement = Vector3.zero;
    }

    [ClientRpc]
    private void ServerPlaceBreak(Vector3 pos, int type, bool isPlacing)
    {
        PlaceBreak(pos, type, isPlacing);
    }

    [Command]
    private void ClientPlaceBreak(Vector3 pos, int type, bool isPlacing)
    {
        ServerPlaceBreak(pos, type, isPlacing);
    }

    private int PlaceBreak(Vector3 pos, int type, bool isPlacing)
    {
        int chunkX = Utils.Floor(pos.x / Chunk.Size),
            chunkZ = Utils.Floor(pos.z / Chunk.Size);
        Chunk chunk = MapHandler.Chunks[chunkX + "." + chunkZ];

        int x = Utils.Floor(pos.x) - chunkX * Chunk.Size,
            y = Utils.Floor(pos.y),
            z = Utils.Floor(pos.z) - chunkZ * Chunk.Size;

        int result = type; // for inventory management
        if (y < 0 || y >= Chunk.Size) return -1; // outside of world height
        if (isPlacing) chunk.Blocks[x, y, z] = type;
        else
        {
            result = chunk.Blocks[x, y, z];
            chunk.Blocks[x, y, z] = 0;
        }
        chunk.BuildMesh();
        
        if(isServer) _mapHandler.SaveChunks(chunk); //save the chunk when modified

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
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // move into or out of the block to get the right targeted block
                hit.point += 0.01f * (right ? 1 : -1) * hit.normal;

                int currentBlock = Inventory.GetCurrentBlock(HotBar.SelectedIndex, 3);
                if (right)
                {
                    int count = Inventory.GetCurrentBlockCount(HotBar.SelectedIndex, 3);
                    if (count <= 0) return;
                    int res = PlaceBreak(hit.point, currentBlock, true); // place the block for this instance
                    if (res != -1) Inventory.RemoveBlock(HotBar.SelectedIndex, 3, Game.InvSprites[0]);
                }
                else
                {
                    int res = PlaceBreak(hit.point, currentBlock, false); // place the block for this instance
                    if (res > 0) Inventory.AddBlock(res, Game.InvSprites[res]); // fixed collecting air

                }

                if (isServer) ServerPlaceBreak(hit.point, currentBlock, right); // server tells clients to place the block
                else ClientPlaceBreak(hit.point, currentBlock, right); // client tells the server to place the block
            }
        }
    }

    void Keys()
    {
        if (Input.GetKeyDown(Settings.KeyMap["Kill"])) Respawn();

        // set spawn
        if (Input.GetKeyDown(Settings.KeyMap["Respawn"])) SetSpawn(transform.position);
    }
    
    void Update()
    {
        if (!isLocalPlayer) return; // don't update other players

        Body.Update(Settings.IsPaused);
        if (Body.OnFloor) GroundedHeight = transform.position.y; // for camera

        _healthImage.transform.localScale = new Vector3(Health,1 ,1);

        bool invVisible = _inventoryUI.inventoryMenu.activeSelf;
        if (Input.GetKeyDown(Settings.KeyMap["Inventory"]))
        {
            if (invVisible) _inventoryUI.HideInventory();
            else if (!Settings.IsPaused) _inventoryUI.DisplayInventory();
        }
        
        // warning: player rotation may be down into the ground
        transform.rotation = Quaternion.Euler(playerCamera.GoalRot);

        // update the following if in-game
        if (!Settings.Playing) return;
        HotBar.UpdateHotBar();
        DetectPlaceBreak();
        Keys();
    }
}