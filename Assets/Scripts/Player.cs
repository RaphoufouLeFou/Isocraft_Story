using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : Mob
{
    private Camera _camera;
    public PlayerCamera playerCamera;

    [NonSerialized] public int Level = 0;
    [NonSerialized] public bool IsLoaded;

    [NonSerialized] public string Name;

    [NonSerialized] public float GroundedHeight; // height at which the player was last grounded
    private Vector3 _spawn;
    private bool _spawnSuccess = true;

    private GameObject _nameTag;
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

        _nameTag = transform.GetChild(3).gameObject;

        if (!isLocalPlayer) return;
        Game.Player = this;
        _nameTag.SetActive(false);
        
        Debug.LogWarning("Saving disabled !");
        SuperGlobals.EditorMode = true;

        Name = SuperGlobals.PlayerName;
        Debug.Log($"My pseudo is : {Name}");
        
        // set up other objects
        GameObject scripts = GameObject.Find("Scripts");
        _inventoryUI = scripts.GetComponent<InventoryUI>();
        _healthImage = GameObject.Find("Health bar").transform.GetChild(0).gameObject;

        _networkManagement = GameObject.Find("NetworkManager").GetComponent<NetworkManagement>();

        Inventory = new Inventory();
        Inventory.AddBlock(Game.Blocks.Cobblestone, Game.InvSprites[Game.Blocks.Cobblestone], 64);

        _inventoryUI.SetPlayerInv(Inventory);
        HotBar.UpdateHotBarVisual(Inventory);

        // body and mob settings
        Transform tr = transform;
        InitMob(Game.Mobs.PlayerMob, new PlayerBody(tr));
        Damage(0); // update health bar at the start
        // start at 0 0, overriden by save pos (temporary, TODO)
        tr.position = new Vector3(0, Chunk.Size, 0);

        // activate camera if needed
        playerCamera.cam.gameObject.SetActive(
            isLocalPlayer && Level == NetworkClient.localPlayer.gameObject.GetComponent<Player>().Level
        );

        IsLoaded = true;
    }

    public void SaveLoaded(Vector3 pos, Vector3 rot, Inventory inv, int health)
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

    public int GetHealth()
    {
        return Health;
    }

    public new void Damage(int amount)
    {
        base.Damage(amount);
        _healthImage.transform.localScale = new Vector3((float)Health / MaxHealth, 1, 1);
    }

    public void SetSpawn(Vector3 pos)
    {
        int y = pos.y < 0 ? 0 : pos.y > Chunk.Size1 ? Chunk.Size1 : (int)pos.y;

        int chunkX = Utils.Floor(pos.x / Chunk.Size), chunkZ = Utils.Floor(pos.z / Chunk.Size);
        _spawn = new Vector3(Utils.Floor(pos.x) + 0.5f, y, Utils.Floor(pos.z) + 0.5f);

        if (MapManager.Chunks != null &&
            MapManager.Chunks.TryGetValue(Chunk.GetName(chunkX, Level, chunkZ), out Chunk chunk))
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
        Body.Stop();
    }

    private int PlaceBreak(int chunkX, int chunkZ, int x, int y, int z, Chunk chunk, int type, bool isPlacing)
    {
        // the slight position change makes the action replace a block or break air
        int result = chunk.Blocks[x, y, z]; // for inventory management
        if (isPlacing ^ chunk.Blocks[x, y, z] == Game.Blocks.Air) return Game.Blocks.None;

        if (!isPlacing && Game.Blocks.FromId[result].Unbreakable) return Game.Blocks.None;

        List<string> update = new List<string> { Chunk.GetName(chunkX, Level, chunkZ) };
        if (x == 0) update.Add(Chunk.GetName(chunkX - 1, Level, chunkZ));
        else if (x == Chunk.Size1) update.Add(Chunk.GetName(chunkX + 1, Level, chunkZ));
        if (z == 0) update.Add(Chunk.GetName(chunkX, Level, chunkZ - 1));
        else if (z == Chunk.Size1) update.Add(Chunk.GetName(chunkX, Level, chunkZ + 1));

        if (isServer) RpcPlaceBreak(update, x, y, z, type, isPlacing);
        else CmdPlaceBreak(update, x, y, z, type, isPlacing);

        return isPlacing ? type : result;
    }

    [ClientRpc]
    private void RpcPlaceBreak(List<string> update, int x, int y, int z, int type, bool isPlacing)
    {
        if (!MapManager.Chunks.ContainsKey(update[0])) return; // map edit isn't visible for this player
        // this could skip an update when editing a chunk border, but this shouldn't matter most of the time
        Chunk chunk = MapManager.Chunks[update[0]];

        // update block in chunk
        if (isPlacing) chunk.Blocks[x, y, z] = type;
        else
        {
            // remove block entity if needed
            if (Game.Blocks.FromId[chunk.Blocks[x, y, z]].IsModel) chunk.RemoveEntity(x, y, z);
            chunk.Blocks[x, y, z] = Game.Blocks.Air;
        }

        // update current chunk, and also nearby chunks if placed on a chunk border
        foreach (string chunkName2 in update)
            if (MapManager.Chunks.ContainsKey(chunkName2))
                MapManager.Chunks[chunkName2].BuildMesh();
    }

    [Command (requiresAuthority = false)]
    private void CmdPlaceBreak(List<string> update, int x, int y, int z, int type, bool isPlacing)
    {
        RpcPlaceBreak(update, x, y, z, type, isPlacing);
    }

    void DetectPlaceBreak()
    {
        bool breaking = Input.GetMouseButtonDown(0), placing = Input.GetMouseButtonDown(1);
        if (breaking || placing)
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // move into or out of the block to get the right targeted block
                Vector3 breakPos = hit.point - 0.01f * hit.normal;
                Vector3 placePos = hit.point + 0.01f * hit.normal;

                // get edition position: more into the block for breaking/interacting, and back a little for placing
                int breakCx = Utils.Floor(breakPos.x / Chunk.Size),
                    breakCz = Utils.Floor(breakPos.z / Chunk.Size);
                int placeCx = Utils.Floor(placePos.x / Chunk.Size),
                    placeCz = Utils.Floor(placePos.z / Chunk.Size);
                int placeX = Utils.Floor(placePos.x) - placeCx * Chunk.Size,
                    placeY = Utils.Floor(placePos.y),
                    placeZ = Utils.Floor(placePos.z) - placeCz * Chunk.Size;
                int breakX = Utils.Floor(breakPos.x) - breakCx * Chunk.Size,
                    breakY = Utils.Floor(breakPos.y),
                    breakZ = Utils.Floor(breakPos.z) - breakCz * Chunk.Size;

                // outside of world
                if (placing && placeY is < 0 or >= Chunk.Size || breaking && breakY is < 0 or >= Chunk.Size) return;

                int currentBlock = Inventory.GetCurrentBlock(HotBar.SelectedIndex, 3);
                // check for breaking and interacting
                if (MapManager.Chunks.TryGetValue(Chunk.GetName(breakCx, Level, breakCz), out Chunk chunk))
                {
                    if (placing && chunk.InteractBlock(breakX, breakY, breakZ)) return; // check for interactions
                    if (breaking) // break block
                    {
                        int res = PlaceBreak(breakCx, breakCz, breakX, breakY, breakZ, chunk, currentBlock, false);
                        if (res >= 0) Inventory.AddBlock(res, Game.InvSprites[res]);
                        return;
                    }
                }

                // check for block placing
                if (placing && MapManager.Chunks.TryGetValue(Chunk.GetName(placeCx, Level, placeCz), out chunk))
                {
                    int count = Inventory.GetCurrentBlockCount(HotBar.SelectedIndex, 3);
                    if (count <= 0) return;
                    int res = PlaceBreak(placeCx, placeCz, placeX, placeY, placeZ, chunk, currentBlock, true);
                    if (res >= 0) Inventory.RemoveBlock(HotBar.SelectedIndex, 3, Game.InvSprites[0]);
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


        // if couldn't spawn before, retry
        if (!_spawnSuccess) SetSpawn(_spawn);
        
        if (!isLocalPlayer){
            _nameTag.transform.rotation = playerCamera.cam.transform.rotation;
            return; // don't update other players
        }
        
        if (Body == null) throw new PlayerException("Player body is null, check game start for errors");


        Body.Update();
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
