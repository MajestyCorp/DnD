using DnD.Model.Inventory;
using DnD.Profiles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

namespace DnD.UI.Inventory
{
    public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        public delegate void ItemHandler(Item item);
        public static event ItemHandler DragStarted;
        public static event ItemHandler DragEnded;

        [SerializeField]
        private Inventory inventory;
        [SerializeField]
        private ItemIconsProfile profile;
        [SerializeField]
        private RectTransform dragContainer;
        [SerializeField]
        private Button button;
        [SerializeField]
        private Image iconImage;
        [SerializeField]
        private Image glowImage;
        [SerializeField]
        private TMP_Text counterField;
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private ScrollRect rootScrollRect;
        [SerializeField]
        private Canvas rootCanvas;

        private Item item;
        private Vector3 originalPosition;
        private RectTransform rect;

        public Item Item => item;

        public void Initialize(Item item)
        {
            Invalidate(item);

            gameObject.SetActive(true);
        }

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            ItemsLibrary.Instance.OnItemUpdated += OnItemUpdated;
            ItemsLibrary.Instance.OnItemDeleted += OnItemDeleted;
        }

        private void OnDisable()
        {
            ItemsLibrary.Instance.OnItemUpdated -= OnItemUpdated;
            ItemsLibrary.Instance.OnItemDeleted -= OnItemDeleted;
        }

        private void OnItemUpdated(Item item)
        {
            if (this.item == null || !this.item.ID.Equals(item.ID))
                return;

            Invalidate(this.item);
        }

        private void OnItemDeleted(Item item)
        {
            if (this.item == null || !this.item.ID.Equals(item.ID))
                return;

            Invalidate(null);
            inventory.Save();
        }

        private void Invalidate(Item item)
        {
            this.item = item;

            button.interactable = item != null;
            iconImage.gameObject.SetActive(button.interactable);
            counterField.gameObject.SetActive(item != null && item.IsConsumable);

            if (item == null)
            {
                name = "cell";
                glowImage.gameObject.SetActive(false);
                return;
            }

            name = item.name;
            glowImage.gameObject.SetActive(item.HasRarityColor());
            if (item.HasRarityColor())
                glowImage.color = item.GetRarityColor();

            iconImage.sprite = null;
            iconImage.sprite = profile.GetIcon(item.icon_group, item.icon_id);

            if (item.IsConsumable)
            {
                counterField.text = item.count.ToString();
            }
        }

        public void ButtonClick()
        {
            if (item == null)
                return;

            SoundManager.Instance.PlayClick();
            TooltipManager.Instance.ShowFor(rect, item, OnDeleteCallback, OnIncCallback, OnEditedCallback);
        }

        private void OnDeleteCallback(Item item)
        {
            Invalidate(null);
            inventory.Save();
        }

        private void OnIncCallback(Item item)
        {
            Invalidate(item);
            inventory.Save();
        }

        private void OnEditedCallback(Item item)
        {
            if (item == null)
                return;

            Invalidate(item);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (item == null)
                return;

            rootScrollRect.enabled = false;
            canvasGroup.blocksRaycasts = false;
            originalPosition = dragContainer.localPosition;
            dragContainer.SetParent(rootCanvas.transform);

            DragStarted?.Invoke(item);
        }

        public void OnDrag(PointerEventData eventData)
        {
            dragContainer.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            rootScrollRect.enabled = true;
            canvasGroup.blocksRaycasts = true;

            ResetIconPosition();

            DragEnded?.Invoke(item);
        }

        private void ResetIconPosition()
        {
            dragContainer.SetParent(transform);
            dragContainer.localPosition = originalPosition;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var drag = eventData.pointerDrag;
            if (drag == null) 
                return;

            var item = drag.GetComponent<InventoryItem>();
            if (item == null || item.Item == null)
                return;

            var item1 = item.Item;
            var item2 = this.Item;

            if (item1 == item2)
                return;

            item.Invalidate(item2);

            this.Invalidate(item1);

            ResetIconPosition();

            inventory.Save();
        }
    }
}