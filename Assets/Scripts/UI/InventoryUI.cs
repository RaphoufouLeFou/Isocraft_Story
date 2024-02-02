using System;
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
    private readonly Sprite[] _cellsSprites = new Sprite[4 * 9];
    private readonly string[] _cellsTexts = new string[4 * 9];

    void Start()
    {
        // get the cells, sprites and texts
        for (int i = 0; i < 4 * 9; i++)
        {
            _cells[i] = content.transform.GetChild(i / 9).GetChild(i % 9);
            _cellsSprites[i] = _cells[i].GetComponent<Image>().sprite;
            _cellsTexts[i] = _cells[i].GetComponentInChildren<TMP_Text>().text;
        }

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
        for (int y = 0; y < 4; y++)
        for (int x = 0; x < 9; x++)
        {
            int i = x + y * 9;
            int count = inventory.GetCurrentBlockCount(x, y);
            if (count > 0) // set the number in the cell
            {
                _cellsSprites[i] = sprites[inventory.GetCurrentBlock(x, y)];
                _cellsTexts[i] = count.ToString();
            }
            else // hide the number in the cell
            {
                _cellsSprites[i] = sprites[0];
                _cellsTexts[i] = "";
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
        if (_isMovingItem)
        {
            _isMovingItem = false;
            Destroy(_movingItemImage);
        }
        Settings.IsPaused = false;
        inventoryMenu.SetActive(false);
    }

    public void UICellButtonListener(GameObject self, string mouse)
    {
        int x = self.transform.GetSiblingIndex();
        int y = self.transform.parent.GetSiblingIndex();
        Inventory inv = _player.GetComponent<Player>().Inventory;
        if (_isMovingItem)
        {
            if (_movingItem.Item1 != inv.GetCurrentBlock(x, y) && inv.GetCurrentBlockCount(x, y) != 0) return;
            int diff = inv.AddBlockAt(x, y, _movingItem.Item1, _movingItem.Item2, _sprites[_movingItem.Item1]);
            if (diff != 0)
            {
                _movingItem.Item2 = diff;
                _movingItemImage.GetComponentInChildren<TMP_Text>().text = diff.ToString();
            }
            else
            {
                _movingItem = (-1, -1);
                Destroy(_movingItemImage);
                _isMovingItem = false;
            }
            UpdateInventory(inv);
        }
        else
        {
            if(inv.GetCurrentBlockCount(x, y) == 0) return;
            if (mouse == "Left")
            {
                _movingItem = (inv.GetCurrentBlock(x, y), inv.GetCurrentBlockCount(x, y));
                inv.RemoveAllBlocks(x, y, _sprites[0]);
            }
            else if (mouse == "Right")
            {
                _movingItem = (inv.GetCurrentBlockCount(x, y) & 1) == 0
                    ? (inv.GetCurrentBlock(x, y), inv.GetCurrentBlockCount(x, y) / 2)
                    : (inv.GetCurrentBlock(x, y), (int)Math.Floor(inv.GetCurrentBlockCount(x, y) / 2.0f + 1));
                inv.RemoveHalfBlocks(x, y, _sprites[0]);
            }
            else if (mouse == "Middle") _movingItem = (inv.GetCurrentBlock(x, y), 64);
            
            _movingItemImage = Instantiate(movingItemImagePrefab, Input.mousePosition, Quaternion.identity, GameObject.Find("Canvas").transform);
            _movingItemImage.GetComponent<Image>().sprite = _sprites[_movingItem.Item1];
            _movingItemImage.GetComponentInChildren<TMP_Text>().text = _movingItem.Item2.ToString();
            UpdateInventory(inv);
            _isMovingItem = true;
        }

        SaveInfos.PlayerInventory = inv;
    }
    
     void Update()
    {
        if (_isMovingItem) _movingItemImage.transform.position = Input.mousePosition;
    }
}
