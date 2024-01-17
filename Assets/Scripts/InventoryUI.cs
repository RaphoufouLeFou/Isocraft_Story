using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryUI : MonoBehaviour, IPointerClickHandler
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
                _movingItemImage.GetComponentInChildren<TMP_Text>().text = $"{diff}";
            }
            else
            {
                _movingItem = (-1, -1);
                Destroy(_movingItemImage);
                _isMovingItem = !_isMovingItem;
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
            else if (mouse == "Middle")
            {
                _movingItem = (inv.GetCurrentBlock(x, y), 64);
            }
            _movingItemImage = Instantiate(movingItemImagePrefab, Input.mousePosition, Quaternion.identity, GameObject.Find("Canvas").transform);
            _movingItemImage.GetComponent<Image>().sprite = _sprites[_movingItem.Item1];
            _movingItemImage.GetComponentInChildren<TMP_Text>().text = $"{_movingItem.Item2}";
            UpdateInventory(inv);
            _isMovingItem = !_isMovingItem;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            Debug.Log("Left click");
        else if (eventData.button == PointerEventData.InputButton.Middle)
            Debug.Log("Middle click");
        else if (eventData.button == PointerEventData.InputButton.Right)
            Debug.Log("Right click");
    }
     void Update()
    {
        if (_isMovingItem)
        {
            _movingItemImage.transform.position = Input.mousePosition;
        }
    }
}
