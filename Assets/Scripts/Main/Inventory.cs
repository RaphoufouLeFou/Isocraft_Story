using System.Collections;
using System.Collections.Generic;
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
    public int[,,] inv = new int[9, 4, 2];
    public Inventory() { }
    public int AddBlock(int block, Sprite texture)
    {
        for (int j = 3; j >= 0; j--) 
        {
            for (int i = 0; i < 9; i++)
            {
                if ((inv[i, j, 0] == block && inv[i, j, 1] < 64) || inv[i, j, 1] == 0)
                {
                    //Debug.Log($"Input block = {block}, current = {inv[i, j, 0]}, quantity = {inv[i, j, 1]}, i = {i}, j = {j}");
                    inv[i, j, 0] = block;
                    inv[i, j, 1]++;
                    if(j == 3) { 
                        Hotbar.ItemImages[i].transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>().sprite = texture;
                        Hotbar.ItemImages[i].transform.GetChild(0).GetChild(0).gameObject.GetComponentInChildren<TMP_Text>().text = $"{inv[i, j, 1]}";
                    }
                    return inv[i, j, 1];
                }
            }
        }
        return -1;
    }

    // Remove 1 block from a slot
    public int RemoveBlock(int x, int y, Sprite texture)
    {
        if (inv[x, y, 1] == 0) return -1;
        else inv[x, y, 1]--;
        if (inv[x, y, 1] == 0)
        {
            Hotbar.ItemImages[Hotbar.selectedIndex].transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>().sprite = texture;
            Hotbar.ItemImages[Hotbar.selectedIndex].transform.GetChild(0).GetChild(0).gameObject.GetComponentInChildren<TMP_Text>().text = "";
        }
        else
            Hotbar.ItemImages[Hotbar.selectedIndex].transform.GetChild(0).GetChild(0).gameObject.GetComponentInChildren<TMP_Text>().text = $"{inv[x, y, 1]}";
        return inv[x, y, 1];
    }

    public int GetCurrentBlockCount(int x, int y)
    {
        return inv[x, y, 1];
    }

    public int GetCurrentBlock(int x, int y)
    {
        return inv[x, y, 0];
    }
}
