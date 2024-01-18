using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public static void UpdateHotBarVisual(Inventory inv, Sprite[] texture)
    {
        for (int i = 0; i < 9; i++)
        {
            Image images = ItemImages[i].transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>();   
            TMP_Text tmpText = ItemImages[i].transform.GetChild(0).GetChild(0).gameObject.GetComponentInChildren<TMP_Text>();
            int type = inv.GetCurrentBlock(i, 3);
            int count = inv.GetCurrentBlockCount(i, 3);

            if (type > 0 && count > 0)
            {
                images.sprite = texture[type]; // set the hotbar texture to the sprite if the block is in the hotbar
                tmpText.text = $"{count}"; // update the hotbar number
            }

        }
    }
}
