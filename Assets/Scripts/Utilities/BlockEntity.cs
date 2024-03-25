using UnityEngine;

public class BlockEntity : Block
{
    private static readonly Vector3 Center = new(0.5f, 0, 0.5f);
    
    public GameObject GameObject;
    private readonly int _id;

    public BlockEntity(int id) : base(null, Game.Blocks.FromId[id].Tags)
    {
        _id = Game.Models.ModelsIndex[id];
    }

    public GameObject GetBaseObject()
    {
        return Game.Models.GameObjects[_id];
    }

    public void SetObject(GameObject go, Vector3 pos)
    {
        GameObject = go;
        GameObject.transform.position = pos + Center - Game.Models.Offsets[_id];
    }
}