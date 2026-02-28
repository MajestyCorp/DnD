using DnD.Model.Inventory;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DnD.UI.Inventory
{
    public class DeleteItemArea : MonoBehaviour, IDropHandler
    {
        [SerializeField]
        private bool deleteAll = true;
        [SerializeField]
        private Button button;
        [SerializeField]
        private Inventory inventory;

        private void Awake()
        {
            InventoryItem.DragStarted += OnDragStarted;
            InventoryItem.DragEnded += OnDragEnded;

            button.interactable = false;
        }

        private void OnDragStarted(Item item)
        {
            if (!deleteAll && (item == null || !item.IsConsumable))
                return;

            button.interactable = true;
        }

        private void OnDragEnded(Item item)
        {
            button.interactable = false;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var drag = eventData.pointerDrag;
            if (drag == null)
                return;

            var item = drag.GetComponent<InventoryItem>();
            if (item == null)
                return;

            if (!deleteAll)
            {
                if (item.Item == null || !item.Item.IsConsumable)
                    return;

                var inventoryItem = item.Item;
                inventoryItem.count--;
                item.Initialize(inventoryItem.count > 0 ? inventoryItem : null);
                inventory.Save();

                return;
            }

            item.Initialize(null);

            inventory.Save();
        }

        private void OnDestroy()
        {
            InventoryItem.DragStarted -= OnDragStarted;
            InventoryItem.DragEnded -= OnDragEnded;
        }
    }
}