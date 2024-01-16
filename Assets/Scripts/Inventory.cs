using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory
{
    /*
     *   Inventory format : x(0) = item, v(1) = quantity
     *   hidden:
     *   [x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v]
     *   [x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v]
     *   [x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v]
     *   hotbar:
     *   [x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v]
     */
    public int[,,] Inv = new int[9, 4, 2]; 
    private Image[] _images = new Image[9]; // cells images
    private TMP_Text[] _tmpText = new TMP_Text[9]; // cells numbers
    public Inventory() {
        // set all cells images and numbers in the hotbar
        for (int i = 0; i < 9; i++)
        {
            _images[i] = Hotbar.ItemImages[i].transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>();   
            _tmpText[i] = Hotbar.ItemImages[i].transform.GetChild(0).GetChild(0).gameObject.GetComponentInChildren<TMP_Text>();
        }
    }
    
    // add one block in the inventory where possible
    public int AddBlock(int block, Sprite texture)
    {
        for (int j = 3; j >= 0; j--) for (int i = 0; i < 9; i++)
            if ((Inv[i, j, 0] == block && Inv[i, j, 1] < 64) || Inv[i, j, 1] == 0)
            {
                Inv[i, j, 0] = block; // set the inventory cell block id to the given id
                Inv[i, j, 1]++; // increment the inventory cell block count by 1
                if(j == 3) {
                    _images[i].sprite = texture; // set the hotbar texture to the sprite if the block is in the hotbar
                    _tmpText[i].text = $"{Inv[i, j, 1]}"; // update the hotbar number
                }
                return Inv[i, j, 1]; // return the updated block count
            }
        return -1;
    }

    // add multiple blocks
    public int AddBlock(int block, Sprite texture, int count)
    {
        for (int j = 3; j >= 0; j--) for (int i = 0; i < 9; i++)
            if ((Inv[i, j, 0] == block && Inv[i, j, 1] + count <= 64) || Inv[i, j, 1] == 0) 
            {
                Inv[i, j, 0] = block; // set the inventory cell block id to the given id
                Inv[i, j, 1]+= count; // increment the inventory cell block count by the given count
                if(j == 3) {
                    _images[i].sprite = texture; // set the hotbar texture to the sprite if the block is in the hotbar
                    _tmpText[i].text = $"{Inv[i, j, 1]}"; // update the hotbar number
                }
                return Inv[i, j, 1]; //return the updated block count
            }
        return -1;
    }
    
    public int AddBlockAt(int x, int y, int block, int count, Sprite texture)
    {
        int remaining = 0;
        Inv[x, y, 0] = block; // increment the inventory cell block count by the given count
        if (count + Inv[x, y, 1] > 64)
        {
            int newCount = 64 - Inv[x, y, 1];
            remaining = count - newCount;
            count = newCount;
        }
        Inv[x, y, 1]+= count; // increment the inventory cell block count by the given count
        if(y == 3) {
            _images[x].sprite = texture; // set the hotbar texture to the sprite if the block is in the hotbar
            _tmpText[x].text = $"{Inv[x, y, 1]}"; // update the hotbar number
        }
        return remaining; //return the updated block count
    }

    // Remove 1 block from a slot
    public void RemoveBlock(int x, int y, Sprite texture)
    {
        if (Inv[x, y, 1] == 0) return; // if the inventory doesn't have a block at the given x and y, return;
        Inv[x, y, 1]--; // remove one block from the cell
        if (Inv[x, y, 1] == 0) // update the hotbar
        {
            _images[x].sprite = texture;
            _tmpText[x].text = "";
        }
        else _tmpText[x].text = $"{Inv[x, y, 1]}";
    }
    
    public void RemoveAllBlocks(int x, int y, Sprite texture)
    {
        if (Inv[x, y, 1] == 0) return;  // if the inventory doesn't have a block at the given x and y, return;
        Inv[x, y, 1] = 0;               // remove one block from the cell
        _images[x].sprite = texture;
        _tmpText[x].text = "";
    }

    public int GetCurrentBlockCount(int x, int y)
    {
        // returns the block count at a given cell
        return Inv[x, y, 1];
    }

    public int GetCurrentBlock(int x, int y)
    {
        // returns the block id at a given cell
        return Inv[x, y, 0];
    }
}
