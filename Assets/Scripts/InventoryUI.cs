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
        for (int j = 0; j < 4; j++)     // iterate through all cells
        for (int i = 0; i < 9; i++)
        {
            cells[j * 9 + i] = content.transform.GetChild(j).GetChild(i); // Get the cell transform
        }
        inventoryMenu.SetActive(false); 
    }
    
    // Display the inventory on the screen
    public void DisplayInventory(Inventory inventory, Sprite[] sprites)
    {
        Settings.IsPaused = true;       
        inventoryMenu.SetActive(true);  
        for (int j = 0; j < 4; j++)     // iterate through all cells
        for (int i = 0; i < 9; i++)
        { 
            cells[j * 9 + i].GetComponent<Image>().sprite = sprites[inventory.GetCurrentBlock(i, j)]; // set the sprite
            int count = inventory.GetCurrentBlockCount(i, j);
            if(count > 0)
                cells[j * 9 + i].GetComponentInChildren<TMP_Text>().text = $"{count}"; // set the number in the cell
            else
                cells[j * 9 + i].GetComponentInChildren<TMP_Text>().text = ""; // hide the nomber in the cell
        }
    } 
    public void HideInventory()
    {
        Settings.IsPaused = false;
        inventoryMenu.SetActive(false);
    } 
}
