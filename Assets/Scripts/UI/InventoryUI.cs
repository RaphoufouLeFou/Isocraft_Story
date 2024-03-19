using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject content;
    public GameObject inventoryMenu;

    private Inventory _playerInv;

    private (int, int) _movingItem;

    public GameObject movingItemImagePrefab;
    private GameObject _movingItemImage;

    private bool _isMovingItem;

    private readonly Transform[] _cells = new Transform[4 * 9];
    private readonly Image[] _cellsImages = new Image[4 * 9];
    private readonly TMP_Text[] _cellsTexts = new TMP_Text[4 * 9];

    void Start()
    {
        // get the cells, sprites and texts
        for (int i = 0; i < 4 * 9; i++)
        {
            _cells[i] = content.transform.GetChild(i / 9).GetChild(i % 9);
            _cellsImages[i] = _cells[i].GetComponent<Image>();
            _cellsTexts[i] = _cells[i].GetComponentInChildren<TMP_Text>();
        }

        inventoryMenu.SetActive(false);
        _isMovingItem = false;
    }

    public void SetPlayerInv(Inventory inv)
    {
        _playerInv = inv;
    }

    // display the inventory on the screen
    public void DisplayInventory()
    {
        inventoryMenu.SetActive(true);
        UpdateInventory();
        Settings.Playing = false;
    }

    private void UpdateInventory()
    {
        for (int y = 0; y < 4; y++)
        for (int x = 0; x < 9; x++)
        {
            int i = x + y * 9;
            int count = _playerInv.GetCurrentBlockCount(x, y);
            if (count > 0) // set the number in the cell
            {
                _cellsImages[i].sprite = Game.InvSprites[_playerInv.GetCurrentBlock(x, y)];
                _cellsTexts[i].text = count.ToString();
            }
            else // hide the number in the cell
            {
                _cellsImages[i].sprite = Game.InvSprites[0];
                _cellsTexts[i].text = "";
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
        inventoryMenu.SetActive(false);
        Settings.Playing = true;
    }

    public void UICellButtonListener(GameObject self, string mouse)
    {
        int x = self.transform.GetSiblingIndex();
        int y = self.transform.parent.GetSiblingIndex();
        if (_isMovingItem)
        {
            if (_movingItem.Item1 != _playerInv.GetCurrentBlock(x, y) && _playerInv.GetCurrentBlockCount(x, y) != 0) return;
            int diff = _playerInv.AddBlockAt(x, y, _movingItem.Item1, _movingItem.Item2, Game.InvSprites[_movingItem.Item1]);
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
        }
        else
        {
            if (_playerInv.GetCurrentBlockCount(x, y) == 0) return;
            if (mouse == "Left")
            {
                _movingItem = (_playerInv.GetCurrentBlock(x, y), _playerInv.GetCurrentBlockCount(x, y));
                _playerInv.RemoveAllBlocks(x, y, Game.InvSprites[0]);
            }
            else if (mouse == "Right")
            {
                _movingItem = (_playerInv.GetCurrentBlockCount(x, y) & 1) == 0
                    ? (_playerInv.GetCurrentBlock(x, y), _playerInv.GetCurrentBlockCount(x, y) / 2)
                    : (_playerInv.GetCurrentBlock(x, y),
                        (int)Math.Floor(_playerInv.GetCurrentBlockCount(x, y) / 2.0f + 1));
                _playerInv.RemoveHalfBlocks(x, y, Game.InvSprites[0]);
            }
            else if (mouse == "Middle") _movingItem = (_playerInv.GetCurrentBlock(x, y), 64);

            _movingItemImage = Instantiate(movingItemImagePrefab, Input.mousePosition, Quaternion.identity,
                GameObject.Find("Canvas").transform);
            _movingItemImage.GetComponent<Image>().sprite = Game.InvSprites[_movingItem.Item1];
            _movingItemImage.GetComponentInChildren<TMP_Text>().text = _movingItem.Item2.ToString();
            _isMovingItem = true;
            //_movingItemPos = new Vector2(x, y);
        }

        UpdateInventory();
    }
    
    void Update()
    {
        if (_isMovingItem) _movingItemImage.transform.position = Input.mousePosition;
    }
}
