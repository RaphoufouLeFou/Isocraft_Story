using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public Sprite[] sprites;
    public GameObject content;
    public GameObject inventoryMenu;
    void Start()
    {
        inventoryMenu.SetActive(false);
    }

    void Update()
    {

    }

    public void DisplayInventory(Inventory inventory)
    {
        inventoryMenu.SetActive(true);
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Transform childTransform = content.transform.GetChild(j).GetChild(j);
                childTransform.GetComponent<Image>().sprite =
                    sprites[inventory.GetCurrentBlock(i, j)];
                childTransform.GetComponentInChildren<TMP_Text>().text = $"{inventory.GetCurrentBlockCount(i, j)}";
            }
        }
    } 
}
