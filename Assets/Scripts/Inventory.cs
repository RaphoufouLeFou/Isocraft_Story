using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory
{
    /*
     *   Inventory format : x(0) = item, v(1) = quantity
     *   hided:
     *   [x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v]
     *   [x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v]
     *   [x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v]
     *   hotbar:
     *   [x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v]
     */
    public int[,,] Inv = new int[9, 4, 2];
    private Image[] _images = new Image[9];
    private TMP_Text[] _tmpText = new TMP_Text[9];
    public Inventory() {
        for (int i = 0; i < 9; i++)
        {
            _images[i] = Hotbar.ItemImages[i].transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>();
            _tmpText[i] = Hotbar.ItemImages[i].transform.GetChild(0).GetChild(0).gameObject.GetComponentInChildren<TMP_Text>();

        }
    }
    public int AddBlock(int block, Sprite texture)
    {
        for (int j = 3; j >= 0; j--) 
        {
            for (int i = 0; i < 9; i++)
            {
                if ((Inv[i, j, 0] == block && Inv[i, j, 1] < 64) || Inv[i, j, 1] == 0)
                {
                    //Debug.Log($"Input block = {block}, current = {inv[i, j, 0]}, quantity = {inv[i, j, 1]}, i = {i}, j = {j}");
                    Inv[i, j, 0] = block;
                    Inv[i, j, 1]++;
                    if(j == 3) {
                        _images[i].sprite = texture;
                        _tmpText[i].text = $"{Inv[i, j, 1]}";
                    }
                    return Inv[i, j, 1];
                }
            }
        }
        return -1;
    }

    // Remove 1 block from a slot
    public void RemoveBlock(int x, int y, Sprite texture)
    {
        if (Inv[x, y, 1] == 0) return;
        Inv[x, y, 1]--;
        if (Inv[x, y, 1] == 0)
        {
            _images[Hotbar.SelectedIndex].sprite = texture;
            _tmpText[Hotbar.SelectedIndex].text = "";
        }
        else
            _tmpText[Hotbar.SelectedIndex].text = $"{Inv[x, y, 1]}";
    }

    public int GetCurrentBlockCount(int x, int y)
    {
        return Inv[x, y, 1];
    }

    public int GetCurrentBlock(int x, int y)
    {
        return Inv[x, y, 0];
    }
}
