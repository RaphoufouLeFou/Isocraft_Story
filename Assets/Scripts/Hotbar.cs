using UnityEngine;

public static class Hotbar
{
    public static readonly GameObject[] ItemImages = new GameObject[9];
    public static int SelectedIndex;

    public static void UpdateHotBar()
    {
        SelectedIndex -= (int)Input.mouseScrollDelta.y;
        SelectedIndex = (SelectedIndex % 9 + 9) % 9; // fit in the hotbar slots

        for(int i = 0; i < 10; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SelectedIndex = i;

        for (int i = 0; i < ItemImages.Length; i++)
        {
            GameObject selectedObject = ItemImages[i].transform.GetChild(0).GetChild(1).gameObject;
            selectedObject.SetActive(i == SelectedIndex);
        }
    }

    public static int GetCurrentBlock(int[,,] inventory)
    {
        if(inventory[SelectedIndex, 3, 1] > 0)
        {
            inventory[SelectedIndex, 3, 1]--;
            return inventory[SelectedIndex, 3, 0];
        }
        return -1; // the player doesn't have the item
    }
}
