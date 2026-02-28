using DnD.Model.Inventory;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DnD.UI.Inventory
{
    public class DeleteLibraryItemArea : MonoBehaviour, IDropHandler
    {
        [SerializeField]
        private Button button;

        private void Awake()
        {
            LibraryItem.DragStarted += OnDragStarted;
            LibraryItem.DragEnded += OnDragEnded;

            button.interactable = false;
        }

        private void OnDragStarted(Item item)
        {
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

            var item = drag.GetComponent<LibraryItem>();
            if (item == null)
                return;

            item.Delete();
        }

        private void OnDestroy()
        {
            LibraryItem.DragStarted -= OnDragStarted;
            LibraryItem.DragEnded -= OnDragEnded;
        }
    }
}