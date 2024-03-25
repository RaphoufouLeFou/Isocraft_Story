using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private Camera _camera;
    public PlayerCamera playerCamera;
    
    [NonSerialized] public int Level = 0;
    [NonSerialized] public float Health;
    [NonSerialized] public bool IsLoaded;

    public CustomRigidBody Body;
    [NonSerialized] public float GroundedHeight; // height at which the player was last grounded
    private Vector3 _spawn;
    private bool _spawnSuccess = true;

    private GameObject _healthImage;
    private InventoryUI _inventoryUI;
    private NetworkManagement _networkManagement;
    public Inventory Inventory;
    
    private void Start()
    {
        // camera
        _camera = GetComponentInChildren<Camera>();
        GetComponentInChildren<AudioListener>().enabled = isLocalPlayer;
        _camera.enabled = isLocalPlayer;

        if (!isLocalPlayer) return;
        Game.Player = this;
        
        // set up other objects
        GameObject scripts = GameObject.Find("Scripts");
        _inventoryUI = scripts.GetComponent<InventoryUI>();
        _healthImage = GameObject.Find("Health bar").transform.GetChild(0).gameObject;
        Health = 1;
        DealDamage(0); // update health bar at the start
        _networkManagement = GameObject.Find("NetworkManager").GetComponent<NetworkManagement>();
        
        Inventory = new Inventory();
        Inventory.AddBlock(Game.Blocks.Cobblestone, Game.InvSprites[Game.Blocks.Cobblestone], 64);
        
        _inventoryUI.SetPlayerInv(Inventory);
        HotBar.UpdateHotBarVisual(Inventory);
        
        // body settings
        Transform tr = transform;
        Body = new CustomRigidBody(tr, 8, 0.9f, 1.3f, -5, 0.95f, 1.85f);

        tr.position = new Vector3(0, Chunk.Size, 0);
        
        // activate camera if needed
        playerCamera.cam.gameObject.SetActive(isLocalPlayer || Level == NetworkClient.localPlayer.gameObject.GetComponent<Player>().Level);
        
        IsLoaded = true;
    }
    
    public void SaveLoaded(Vector3 pos, Vector3 rot, Inventory inv, float health)
    {
        // set variables once save infos are loaded
        SetSpawn(pos);
        Health = health;
        playerCamera.GoalRot.y = MathF.Round(rot.y / 45) * 45;
        playerCamera.GoToPlayer();
        Inventory = inv;
        _inventoryUI.SetPlayerInv(Inventory);
        HotBar.UpdateHotBarVisual(Inventory);
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
                if (searchY is >= 1 and < Chunk.Size
                    && chunk.Blocks[modX, (int)searchY - 1, modZ] != 0
                    && chunk.Blocks[modX, (int)searchY, modZ] == 0)
                {
                    _spawn.y = searchY;
                    break;
                }
            }
            
            _spawn.y++; // player needs to be one block above the ground
            _spawnSuccess = true;
            Respawn();
        }

        else // couldn't find the chunk to spawn in, just wait until it's spawned
        {
            _spawn = pos;
            _spawnSuccess = false;
        }
    }

    private void Respawn()
    {
        transform.position = _spawn;
        Body.Movement = Vector3.zero;
    }

    public void DealDamage(float amount)
    {
        Health = amount < Health ? Health - amount : 0;
        _healthImage.transform.localScale = new Vector3(Health,1 ,1);
    }
    
    private int PlaceBreak(Vector3 pos, int type, bool isPlacing)
    {
        int chunkX = Utils.Floor(pos.x / Chunk.Size),
            chunkZ = Utils.Floor(pos.z / Chunk.Size);

        // chunk is not loaded, current player cannot edit
        string chunkName = $"{chunkX}.{chunkZ}";
        if (!MapHandler.Chunks.ContainsKey(chunkName)) return -1;
        Chunk chunk = MapHandler.Chunks[chunkName];

        int x = Utils.Floor(pos.x) - chunkX * Chunk.Size,
            y = Utils.Floor(pos.y),
            z = Utils.Floor(pos.z) - chunkZ * Chunk.Size;

        if (y is < 0 or >= Chunk.Size) return -1; // outside of world
        
        // the slight position change makes the action replace a block or break air
        int result = chunk.Blocks[x, y, z]; // for inventory management
        if (isPlacing ^ chunk.Blocks[x, y, z] == Game.Blocks.Air) return -1;

        if (!isPlacing && Game.Blocks.FromId[result].Unbreakable) return -1; // can't break bedrock

        List<string> update = new List<string> { chunkName };
        if (x == 0) update.Add(chunkX - 1 + "." + chunkZ);
        else if (x == Chunk.Size1) update.Add(chunkX + 1 + "." + chunkZ);
        if (z == 0) update.Add(chunkX + "." + (chunkZ - 1));
        else if (z == Chunk.Size1) update.Add(chunkX + "." + (chunkZ + 1));

        if (isServer)
        {
            RpcPlaceBreak(update, x, y, z, type, isPlacing);
            foreach (string chunkName2 in update) MapHandler.SaveChunk(MapHandler.Chunks[chunkName2]);
        }
        else CmdPlaceBreak(update, x, y, z, type, isPlacing);
        
        return isPlacing ? type : result;
    }

    [ClientRpc]
    private void RpcPlaceBreak(List<string> update, int x, int y, int z, int type, bool isPlacing)
    {
        if (!MapHandler.Chunks.ContainsKey(update[0])) return; // map edit isn't visible for this player
        Chunk chunk = MapHandler.Chunks[update[0]];
        
        // update block in chunk
        if (isPlacing) chunk.Blocks[x, y, z] = type;
        else chunk.Blocks[x, y, z] = Game.Blocks.Air;
       
        // update current chunk, and also nearby chunks if placed on a chunk border
        foreach (string chunkName2 in update)
            if (MapHandler.Chunks.ContainsKey(chunkName2))
                MapHandler.Chunks[chunkName2].BuildMesh();
    }
    
    [Command (requiresAuthority = false)]
    private void CmdPlaceBreak(List<string> update, int x, int y, int z, int type, bool isPlacing)
    {
        RpcPlaceBreak(update, x, y, z, type, isPlacing);
        foreach (string chunkName2 in update) MapHandler.SaveChunk(MapHandler.Chunks[chunkName2]);
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
                hit.point += (right ? 0.01f : -0.01f) * hit.normal;

                int currentBlock = Inventory.GetCurrentBlock(HotBar.SelectedIndex, 3);
                if (right)
                {
                    int count = Inventory.GetCurrentBlockCount(HotBar.SelectedIndex, 3);
                    if (count <= 0) return;
                    int res = PlaceBreak(hit.point, currentBlock, true); // place the block for this instance
                    if (res >= 0) Inventory.RemoveBlock(HotBar.SelectedIndex, 3, Game.InvSprites[0]);
                }
                else
                {
                    int res = PlaceBreak(hit.point, currentBlock, false); // place the block for this instance
                    if (res >= 0) Inventory.AddBlock(res, Game.InvSprites[res]);
                }
            }
        }
    }

    void Keys()
    {
        if (Input.GetKeyDown(Settings.KeyMap["Kill"])) Respawn();
        if (Input.GetKeyDown(Settings.KeyMap["Respawn"])) SetSpawn(transform.position);
    }
    
    void Update()
    {
        if (Body == null) throw new PlayerException("Player body is null, check game start for errors");
        
        // if couldn't spawn before, retry
        if (!_spawnSuccess) SetSpawn(_spawn);

        if (!isLocalPlayer) return; // don't update other players
        
        Body.Update(Settings.IsPaused);
        if (Body.OnFloor) GroundedHeight = transform.position.y; // for camera

        bool invVisible = _inventoryUI.inventoryMenu.activeSelf;
        if (Input.GetKeyDown(Settings.KeyMap["Inventory"]))
        {
            if (invVisible) _inventoryUI.HideInventory();
            else if (!Settings.IsPaused) _inventoryUI.DisplayInventory();
        }
        
        // warning: the tilted camera rotation can make the player look down
        transform.rotation = Quaternion.Euler(playerCamera.GoalRot);

        // update the following if in-game
        if (!Settings.Playing) return;
        HotBar.UpdateHotBar();
        DetectPlaceBreak();
        Keys();
    }
    
    public override void OnStopClient()
    {
        base.OnStopClient();
        if (isServer)
        {
            Debug.LogWarning("probably an issue when there are multiple clients !! TODO");
            return;
        }
        _networkManagement.LeaveGame();
    }
}