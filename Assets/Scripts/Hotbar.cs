using UnityEngine;

public static class Hotbar
{
    public static GameObject[] ItemImages = new GameObject[9];
    public static int SelectedIndex;



    public static void UpdateHotBar()
    {

        SelectedIndex -= (int)Input.mouseScrollDelta.y;
        while (SelectedIndex > 8) SelectedIndex -= 9;
        while (SelectedIndex < 0) SelectedIndex += 9;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectedIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectedIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectedIndex = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectedIndex = 3;
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectedIndex = 4;
        if (Input.GetKeyDown(KeyCode.Alpha6)) SelectedIndex = 5;
        if (Input.GetKeyDown(KeyCode.Alpha7)) SelectedIndex = 6;
        if (Input.GetKeyDown(KeyCode.Alpha8)) SelectedIndex = 7;
        if (Input.GetKeyDown(KeyCode.Alpha9)) SelectedIndex = 8;

        for (int i = 0; i < ItemImages.Length; i++)
        {
            GameObject selectedObject = ItemImages[i].transform.GetChild(0).GetChild(1).gameObject;
            if (i == SelectedIndex) selectedObject.SetActive(true);
            else selectedObject.SetActive(false);
        }
    }

    public static int GetCurentBlock(int[,,] inventry)
    {
        if(inventry[SelectedIndex, 3, 1] > 0)
        {
            inventry[SelectedIndex, 3, 1]--;
            return inventry[SelectedIndex, 3, 0];
        }
        return -1;  // the player don't have the item
    }
}
