using UnityEngine;

public static class Hotbar
{
    public static GameObject[] ItemImages = new GameObject[9];
    public static int selectedIndex = 0;



    public static void updateHotBar()
    {

        selectedIndex -= (int)Input.mouseScrollDelta.y;
        while (selectedIndex > 8) selectedIndex -= 9;
        while (selectedIndex < 0) selectedIndex += 9;

        if (Input.GetKeyDown(KeyCode.Alpha1)) selectedIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) selectedIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) selectedIndex = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) selectedIndex = 3;
        if (Input.GetKeyDown(KeyCode.Alpha5)) selectedIndex = 4;
        if (Input.GetKeyDown(KeyCode.Alpha6)) selectedIndex = 5;
        if (Input.GetKeyDown(KeyCode.Alpha7)) selectedIndex = 6;
        if (Input.GetKeyDown(KeyCode.Alpha8)) selectedIndex = 7;
        if (Input.GetKeyDown(KeyCode.Alpha9)) selectedIndex = 8;

        for (int i = 0; i < ItemImages.Length; i++)
        {
            GameObject selectedObject = ItemImages[i].transform.GetChild(0).GetChild(1).gameObject;
            if (i == selectedIndex) selectedObject.SetActive(true);
            else selectedObject.SetActive(false);
        }
    }

    public static int GetCurentBlock(int[,,] inventry)
    {
        if(inventry[selectedIndex, 3, 1] > 0)
        {
            inventry[selectedIndex, 3, 1]--;
            return inventry[selectedIndex, 3, 0];
        }
        return -1;  // the player don't have the item
    }
}
