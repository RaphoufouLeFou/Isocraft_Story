using System;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject content;
    public GameObject inventoryMenu;

    private GameObject _player;

    private Sprite[] _sprites;

    private (int, int) _movingItem;

    public GameObject movingItemImagePrefab;
    private GameObject _movingItemImage;

    private bool _isMovingItem;

    private readonly Transform[] _cells = new Transform[4 * 9];
    void Start()
    {
        // iterate through all cells to get the cell transform
        for (int j = 0; j < 4; j++)
        for (int i = 0; i < 9; i++)
            _cells[j * 9 + i] = content.transform.GetChild(j).GetChild(i);
        inventoryMenu.SetActive(false);
        _isMovingItem = false;
    }
    
    // display the inventory on the screen
    public void DisplayInventory(Inventory inventory, Sprite[] sprites, GameObject player)
    {
        _player = player;
        _sprites = sprites;
        Settings.IsPaused = true;       
        inventoryMenu.SetActive(true);  
        for (int j = 0; j < 4; j++) // iterate through all cells
        for (int i = 0; i < 9; i++)
        { 
            int count = inventory.GetCurrentBlockCount(i, j);
            if (count > 0) // set the number in the cell
            {
                _cells[j * 9 + i].GetComponent<Image>().sprite = sprites[inventory.GetCurrentBlock(i, j)]; // set the sprite
                _cells[j * 9 + i].GetComponentInChildren<TMP_Text>().text = $"{count}";
            }
            else // hide the number in the cell
            {
                _cells[j * 9 + i].GetComponent<Image>().sprite = sprites[0]; // set the sprite
                _cells[j * 9 + i].GetComponentInChildren<TMP_Text>().text = "";
            }
        }
    }

    private void UpdateInventory(Inventory inventory)
    {
        
        for (int j = 0; j < 4; j++) // iterate through all cells
        for (int i = 0; i < 9; i++)
        { 
            
            int count = inventory.GetCurrentBlockCount(i, j);
            if (count > 0) // set the number in the cell
            {
                _cells[j * 9 + i].GetComponent<Image>().sprite = _sprites[inventory.GetCurrentBlock(i, j)]; // set the sprite
                _cells[j * 9 + i].GetComponentInChildren<TMP_Text>().text = $"{count}";
            }
            else // hide the number in the cell
            {
                _cells[j * 9 + i].GetComponent<Image>().sprite = _sprites[0]; // set the sprite
                _cells[j * 9 + i].GetComponentInChildren<TMP_Text>().text = "";
            }
        }
    }
    public void HideInventory()
    {
        if (_movingItemImage != null)
        {
            _isMovingItem = false;
            Destroy(_movingItemImage);
        }
        Settings.IsPaused = false;
        inventoryMenu.SetActive(false);
    }

    public void UICellButtonListener(GameObject self)
    {
        int x = self.transform.GetSiblingIndex();
        int y = self.transform.parent.GetSiblingIndex();
        Inventory inv = _player.GetComponent<Player>().Inventory;
        if (_isMovingItem)
        {
            inv.AddBlockAt(x, y, _movingItem.Item1, _movingItem.Item2, _sprites[_movingItem.Item1]);
            _movingItem = (-1, -1);
            Destroy(_movingItemImage);
            UpdateInventory(inv);
            _isMovingItem = !_isMovingItem;
        }
        else
        {
            if(inv.GetCurrentBlockCount(x, y) == 0) return;
            _movingItem = (inv.GetCurrentBlock(x, y), inv.GetCurrentBlockCount(x, y));
            inv.RemoveAllBlocks(x, y, _sprites[0]);
            _movingItemImage = Instantiate(movingItemImagePrefab, Input.mousePosition, Quaternion.identity, GameObject.Find("Canvas").transform);
            _movingItemImage.GetComponent<Image>().sprite = _sprites[_movingItem.Item1];
            _movingItemImage.GetComponentInChildren<TMP_Text>().text = $"{_movingItem.Item2}";
            UpdateInventory(inv);
            _isMovingItem = !_isMovingItem;
        }
        
    }

     void Update()
    {
        if (_isMovingItem)
        {
            _movingItemImage.transform.position = Input.mousePosition;
        }
    }
}
