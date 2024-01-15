using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject content;
    public GameObject inventoryMenu;
    void Start()
    {
        inventoryMenu.SetActive(false);
    }

    void Update()
    {

    }

    public void DisplayInventory(Inventory inventory, Sprite[] sprites)
    {
        Settings.IsPaused = true;
        inventoryMenu.SetActive(true);
        for (int j = 0; j < 4; j++)
        for (int i = 0; i < 9; i++)
        { 

            Transform childTransform = content.transform.GetChild(j).GetChild(i);
            
            childTransform.GetComponent<Image>().sprite = sprites[inventory.GetCurrentBlock(i, j)];
            int count = inventory.GetCurrentBlockCount(i, j);
            if(count > 0)
                childTransform.GetComponentInChildren<TMP_Text>().text = $"{count}";
            else
                childTransform.GetComponentInChildren<TMP_Text>().text = "";
        }
    } 
    public void HideInventory()
    {
        Settings.IsPaused = false;
        inventoryMenu.SetActive(false);
    } 
}
