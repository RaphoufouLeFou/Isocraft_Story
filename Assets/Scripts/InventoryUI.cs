using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject content;
    public GameObject inventoryMenu;

    private Transform[]  cells = new Transform[4 * 9];
    void Start()
    {
        // iterate through all cells to get the cell transform
        for (int j = 0; j < 4; j++)
        for (int i = 0; i < 9; i++)
            cells[j * 9 + i] = content.transform.GetChild(j).GetChild(i);
        inventoryMenu.SetActive(false); 
    }
    
    // display the inventory on the screen
    public void DisplayInventory(Inventory inventory, Sprite[] sprites)
    {
        Settings.IsPaused = true;       
        inventoryMenu.SetActive(true);  
        for (int j = 0; j < 4; j++) // iterate through all cells
        for (int i = 0; i < 9; i++)
        { 
            
            int count = inventory.GetCurrentBlockCount(i, j);
            if (count > 0) // set the number in the cell
            {
                cells[j * 9 + i].GetComponent<Image>().sprite = sprites[inventory.GetCurrentBlock(i, j)]; // set the sprite
                cells[j * 9 + i].GetComponentInChildren<TMP_Text>().text = $"{count}";
            }
            else // hide the number in the cell
            {
                cells[j * 9 + i].GetComponent<Image>().sprite = sprites[0]; // set the sprite
                cells[j * 9 + i].GetComponentInChildren<TMP_Text>().text = "";
            }
        }
    } 
    public void HideInventory()
    {
        Settings.IsPaused = false;
        inventoryMenu.SetActive(false);
    } 
}
