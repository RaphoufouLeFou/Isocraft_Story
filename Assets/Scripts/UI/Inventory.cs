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
     *   hotBar:
     *   [x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v][x,v]
     */
    public readonly int[,,] Inv = new int[9, 4, 2];
    private static readonly Image[] Images = new Image[9]; // cells images
    private static readonly TMP_Text[] TmpText = new TMP_Text[9]; // cells numbers

    public static void Init()
    {
        // set all cells images and numbers in the hotBar
        for (int i = 0; i < 9; i++)
        {
            GameObject go = HotBar.ItemImages[i].transform.GetChild(0).GetChild(0).gameObject;
            Images[i] = go.GetComponent<Image>();
            TmpText[i] = go.GetComponentInChildren<TMP_Text>();
        }
    }

    // add one block in the inventory where possible
    public void AddBlock(int block, Sprite texture)
    {
        if (block == Game.Blocks.Air) return;

        for (int j = 3; j >= 0; j--) for (int i = 0; i < 9; i++)
            if ((Inv[i, j, 0] == block && Inv[i, j, 1] < 64) || Inv[i, j, 1] == 0)
            {
                Inv[i, j, 0] = block; // set the inventory cell block id to the given id
                Inv[i, j, 1]++; // increment the inventory cell block count by 1
                if (j == 3)
                {
                    Images[i].sprite = texture; // set the hotBar texture to the sprite if the block is in the hotBar
                    TmpText[i].text = Inv[i, j, 1].ToString(); // update the hotBar number
                }

                return;
            }
    }

    // add multiple blocks
    public void AddBlock(int block, Sprite texture, int count)
    {
        for (int j = 3; j >= 0; j--) for (int i = 0; i < 9; i++)
            if ((Inv[i, j, 0] == block && Inv[i, j, 1] + count <= 64) || Inv[i, j, 1] == 0)
            {
                Inv[i, j, 0] = block; // set the inventory cell block id to the given id
                Inv[i, j, 1] += count; // increment the inventory cell block count by the given count
                if (j == 3)
                {
                    Images[i].sprite = texture; // set the hotBar texture to the sprite if the block is in the hotBar
                    TmpText[i].text = Inv[i, j, 1].ToString(); // update the hotBar number
                }
                return;
            }
    }

    public override string ToString()
    {
        string text = "";
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                text += $"{Inv[j,i,0]}:{Inv[j,i,0]} ";
            }
            text += "\n";
        }

        return text;
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
        Inv[x, y, 1] += count; // increment the inventory cell block count by the given count
        if (y == 3)
        {
            Images[x].sprite = texture; // set the hotBar texture to the sprite if the block is in the hotBar
            TmpText[x].text = Inv[x, y, 1].ToString(); // update the hotBar number
        }
        return remaining; //return the updated block count
    }

    public int AddBlockAt(int x, int y, int block, int count)
    {
        int remaining = 0;
        Inv[x, y, 0] = block; // increment the inventory cell block count by the given count
        if (count + Inv[x, y, 1] > 64)
        {
            int newCount = 64 - Inv[x, y, 1];
            remaining = count - newCount;
            count = newCount;
        }
        Inv[x, y, 1] += count; // increment the inventory cell block count by the given count
        return remaining; // return the updated block count
    }

    // Remove 1 block from a slot
    public void RemoveBlock(int x, int y, Sprite texture)
    {
        if (Inv[x, y, 1] == 0) return; // if the inventory doesn't have a block at the given x and y, return;
        Inv[x, y, 1]--; // remove one block from the cell
        if (Inv[x, y, 1] == 0) // update the hotBar
        {
            Images[x].sprite = texture;
            TmpText[x].text = "";
        }
        else TmpText[x].text = Inv[x, y, 1].ToString();
    }

    public void RemoveHalfBlocks(int x, int y, Sprite texture)
    {
        if (Inv[x, y, 1] == 1) RemoveAllBlocks(x, y, texture);
        if (Inv[x, y, 1] == 0) return; // if the inventory doesn't have a block at the given x and y, return;
        Inv[x, y, 1] /= 2; // remove one block from the cell
        if (y == 3)
            TmpText[x].text = Inv[x, y, 1].ToString();
    }

    public void RemoveAllBlocks(int x, int y, Sprite texture)
    {
        if (Inv[x, y, 1] == 0) return;
        Inv[x, y, 1] = 0;
        if (y == 3)
        {
            Images[x].sprite = texture;
            TmpText[x].text = "";
        }
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
