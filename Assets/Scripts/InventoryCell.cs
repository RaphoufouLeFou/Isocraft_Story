using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryCell : MonoBehaviour, IPointerClickHandler
{
    public InventoryUI inventoryUI;
    public GameObject self;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            inventoryUI.UICellButtonListener(self, "Left");
        else if (eventData.button == PointerEventData.InputButton.Middle)
            inventoryUI.UICellButtonListener(self, "Middle");
        else if (eventData.button == PointerEventData.InputButton.Right)
            inventoryUI.UICellButtonListener(self, "Right");
    }
}
