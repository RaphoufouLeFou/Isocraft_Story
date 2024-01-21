using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public new Camera camera;
    public PlayerCamera playerCamera;

    [NonSerialized] public CustomRigidBody Body;
    [NonSerialized] public float GroundedHeight; // height at which the player was last grounded
    [NonSerialized] public Vector3 Spawn;

    private GameObject _healthImage;

    private GameObject _scriptsGameObject;

    public float health; // from 0 to 1

    public Inventory Inventory;
    
    private Sprite[] _sprites;

    void Start()
    {
        camera.enabled = isLocalPlayer;
        if (!isLocalPlayer) return;

        // set up objects
        _scriptsGameObject = GameObject.Find("Scripts");
        _sprites = _scriptsGameObject.GetComponent<Game>().sprites;
        _healthImage = GameObject.Find("Health bar").transform.GetChild(0).gameObject;
        //camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        GameObject items = GameObject.Find("HotBarBackground");

        health = 1.0f;
        for (int i = 0; i < 9; i++) Hotbar.ItemImages[i] = items.transform.GetChild(i).gameObject;
        if (SaveInfos.HasBeenLoaded)
        {
            Inventory = SaveInfos.PlayerInventory;
            Inventory.InitInventory();
        }
        else
        {
            Inventory = new ();
            Inventory.InitInventory();
            Inventory.AddBlock(Game.Blocks.Cobblestone, _sprites[Game.Blocks.Cobblestone], 64);
            SaveInfos.PlayerInventory = Inventory;
        }
        
        Hotbar.UpdateHotBarVisual(Inventory, _sprites);

        // body settings
        Transform tr = transform;
        Body = new CustomRigidBody(tr, 8, 0.9f, 1.3f, -5, 0.95f, 1.85f);
        if (SaveInfos.HasBeenLoaded)
        {
            SetSpawn(SaveInfos.PlayerPosition);
            transform.eulerAngles = new Vector3(0,SaveInfos.PlayerRotation.y,0);
            playerCamera.GoalRot.y = SaveInfos.PlayerRotation.y;
        }
        else SetSpawn(new Vector3(0, Chunk.Size, 0));
        tr.position = Spawn;

    }
    public void SetSpawn(Vector3 pos)
    {
        int y = pos.y < 0 ? 0 : pos.y >= Chunk.Size ? Chunk.Size1 : (int)pos.y;

        int chunkX = Utils.Floor(pos.x / Chunk.Size), chunkZ = Utils.Floor(pos.z / Chunk.Size);
        Spawn = new Vector3(Utils.Floor(pos.x) + 0.5f, y, Utils.Floor(pos.z) + 0.5f);
        if (MapHandler.Chunks.TryGetValue(chunkX + "." + chunkZ, out Chunk chunk))
        {
            int modX = (int)(pos.x - chunkX * Chunk.Size),
                modZ = (int)(pos.z - chunkZ * Chunk.Size);
            // try to find a height near Spawn.y
            for (int offset = 0; offset < Chunk.Size << 1; offset++)
            {
                float searchY = Spawn.y + (offset >> 1) * ((offset & 1) == 1 ? -1 : 1);
                if (chunk.Blocks[modX, (int)searchY, modZ] == 0)
                {
                    Spawn.y = searchY;
                    break;
                }
            }
        }
        else throw new ArgumentException("Cannot set spawn in unloaded chunk");

        Spawn.y++; // player needs to be one block above
    }

    [ClientRpc]
    void ServerPlaceBreak(Vector3 pos, int type, bool isPlacing)
    {
        PlaceBreak(pos, type, isPlacing);
    }

    [Command]
    void ClientPlaceBreak(Vector3 pos, int type, bool isPlacing)
    {
        ServerPlaceBreak(pos, type, isPlacing);
    }

    int PlaceBreak(Vector3 pos, int type, bool isPlacing)
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
        
        if(isServer) GameObject.Find("MapHandler").GetComponent<MapHandler>().SaveChunks(chunk); //save the chunk when modified

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
                    if (res != -1) Inventory.RemoveBlock(Hotbar.SelectedIndex, 3, _sprites[0]);
                }
                else
                {
                    int res = PlaceBreak(hit.point, currentBlock, false); // place the block for this instance
                    if (res > 0) Inventory.AddBlock(res, _sprites[res]); // fixed collecting air

                }

                if (isServer) ServerPlaceBreak(hit.point, currentBlock, right); // server tells clients to place the block
                else ClientPlaceBreak(hit.point, currentBlock, right); // client tells the server to place the block
            }
        }

        SaveInfos.PlayerInventory = Inventory;
    }

    void DetectOtherActions()
    {
        // rotate camera about the Y axis
        Vector3 rotation = transform.rotation.eulerAngles;
        bool change = false;
        if (Input.GetKeyDown(Settings.KeyMap["CamLeft"]))
        {
            change = true;
            rotation.y -= 45;
        }
        if (Input.GetKeyDown(Settings.KeyMap["CamRight"]))
        {
            change = true;
            rotation.y += 45;
        }

        // toggle camera target X rotation
        if (Input.GetKeyDown(Settings.KeyMap["TopView"])) playerCamera.TargetAbove = !playerCamera.TargetAbove;
        
        if (change) playerCamera.GoalRot.y = rotation.y;
        transform.rotation = Quaternion.Euler(rotation);
        
        if (Input.GetKeyDown(Settings.KeyMap["Kill"])) // kill
        {
            transform.position = Spawn;
            Body.Movement = Vector3.zero;
        }

        Vector3 pos = transform.position;
        // set spawn
        if (Input.GetKeyDown(Settings.KeyMap["Respawn"])) SetSpawn(pos);
    }
    
    void Update()
    {
        if (!isLocalPlayer) return;

        if (health > 1) health = 1;
        _healthImage.transform.localScale = new Vector3(health,1 ,1);
        Body.Update(Settings.IsPaused);
        SaveInfos.PlayerPosition = transform.position;
        if (Body.OnFloor) GroundedHeight = transform.position.y; // for camera

        if (Input.GetKeyDown(Settings.KeyMap["Inventory"]) || (Input.GetKeyDown(KeyCode.Escape) && _scriptsGameObject.GetComponent<InventoryUI>().inventoryMenu.activeSelf))
        {
            InventoryUI inventoryUI = _scriptsGameObject.GetComponent<InventoryUI>();
            if(Settings.IsPaused && inventoryUI.inventoryMenu.activeSelf)
                inventoryUI.HideInventory();
            else
                inventoryUI.DisplayInventory(Inventory, _sprites, gameObject);
        }
        
        // update these if not paused
        if (Settings.IsPaused) return;

        Hotbar.UpdateHotBar();
        DetectPlaceBreak();
        DetectOtherActions();
    }
}
