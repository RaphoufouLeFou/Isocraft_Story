using UnityEngine;

public class Chest : BlockEntity, IBlockEntity
{
    private bool _isOpened;
    public int[,,] Content = new int[9, 3, 2];

    public Chest(int id) : base(id)
    {
        
    }

    public new void Interact()
    {
        Debug.Log("Interacted with chest");
    }
}