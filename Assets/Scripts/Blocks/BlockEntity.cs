using UnityEngine;

public interface IBlockEntity
{
    public void Interact();
}

public class BlockEntity : Block, IBlockEntity
{
    private static readonly Vector3 Center = new(0.5f, 0, 0.5f);
    
    private GameObject _gameObject;
    private readonly int _id;

    public static BlockEntity Create(int id)
    {
        switch (id)
        {
            case Game.Blocks.Chest: return new Chest(id);
            default: return new BlockEntity(id);
        }
    }
    
    protected BlockEntity(int id) : base(null, Game.Blocks.FromId[id].Tags)
    {
        _id = Game.Models.ModelsIndex[id];
    }

    public GameObject GetBaseObject()
    {
        return Game.Models.GameObjects[_id];
    }

    public void SetObject(GameObject go, Vector3 pos)
    {
        // place the object in the center of the block
        _gameObject = go;
        _gameObject.transform.position = pos + Center - Game.Models.Offsets[_id];
    }

    public void Interact()
    {
        throw new BlockException("Base block entity cannot be interacted with");
    }
}