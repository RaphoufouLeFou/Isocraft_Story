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
        inventoryMenu.SetActive(true);
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Transform childTransform = content.transform.GetChild(j).GetChild(j);
                int index = inventory.GetCurrentBlock(i, j);
                childTransform.GetComponent<Image>().sprite = sprites[index];
                childTransform.GetComponentInChildren<TMP_Text>().text = $"{inventory.GetCurrentBlockCount(i, j)}";
            }
        }
    } 
}
