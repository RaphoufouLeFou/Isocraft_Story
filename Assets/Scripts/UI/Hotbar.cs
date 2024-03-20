using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class HotBar
{
    public static GameObject[] ItemImages = new GameObject[9];
    public static int SelectedIndex;

    public static void InitImages()
    {
        GameObject items = GameObject.Find("HotBarBackground");
        for (int i = 0; i < 9; i++) HotBar.ItemImages[i] = items.transform.GetChild(i).gameObject;
    }
    
    public static void UpdateHotBar()
    {
        SelectedIndex -= (int)Input.mouseScrollDelta.y;
        SelectedIndex = (SelectedIndex % 9 + 9) % 9; // fit in the hotBar slots

        for(int i = 0; i < 10; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SelectedIndex = i;

        for (int i = 0; i < ItemImages.Length; i++)
        {
            GameObject selectedObject = ItemImages[i].transform.GetChild(0).GetChild(1).gameObject;
            selectedObject.SetActive(i == SelectedIndex);
        }
    }

    public static void UpdateHotBarVisual(Inventory inv)
    {
        for (int i = 0; i < 9; i++)
        {
            Image images = ItemImages[i].transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>();
            TMP_Text tmpText = ItemImages[i].transform.GetChild(0).GetChild(0).gameObject
                .GetComponentInChildren<TMP_Text>();
            int type = inv.GetCurrentBlock(i, 3);
            int count = inv.GetCurrentBlockCount(i, 3);

            if (type > 0 && count > 0)
            {
                images.sprite = Game.InvSprites[Mathf.Min(type, Game.InvSprites.Length - 1)];
                tmpText.text = count.ToString();
            }
        }
    }

    public static void SetScale(float scale)
    {
        GameObject items = GameObject.Find("Hotbar");
        items.transform.GetChild(0).localScale = new Vector3(scale, scale, scale);
    }
}
