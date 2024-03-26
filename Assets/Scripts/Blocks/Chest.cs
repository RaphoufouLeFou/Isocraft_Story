using UnityEngine;

public class Chest : BlockEntity, IBlockEntity
{
    private bool _isOpened;
    private int[,] _content = new int[9, 3];

    public Chest(int id) : base(id)
    {
        
    }

    public new void Interact()
    {
        Debug.Log("Interacted with chest");
    }
}