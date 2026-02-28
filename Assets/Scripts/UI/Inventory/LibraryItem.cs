using DnD.Model.Inventory;
using DnD.Profiles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

namespace DnD.UI.Inventory
{
    public class LibraryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        public delegate void ItemHandler(Item item);
        public static event ItemHandler DragStarted;
        public static event ItemHandler DragEnded;

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
        private CanvasGroup canvasGroup;

        [SerializeField]
        private ScrollRect rootScrollRect;
        [SerializeField]
        private Canvas rootCanvas;

        private Item item;
        private Vector3 originalPosition;
        private Action<Item> onSelectCallback;
        private Action<Item> onEditedCallback;
        private LibraryGroup group;
        private RectTransform rect;

        public Item Item => item;

        public void Initialize(LibraryGroup group, Item item, Action<Item> onSelectCallback, Action<Item> onEditedCallback)
        {
            this.group = group;
            this.onSelectCallback = onSelectCallback;
            this.onEditedCallback = onEditedCallback;

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
        }

        private void OnDisable()
        {
            ItemsLibrary.Instance.OnItemUpdated -= OnItemUpdated;
        }

        private void OnItemUpdated(Item item)
        {
            if (this.item == null || !this.item.ID.Equals(item.ID))
                return;

            Invalidate(this.item);
        }

        private void Invalidate(Item item)
        {
            this.item = item;

            button.interactable = item != null;
            iconImage.gameObject.SetActive(button.interactable);

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
        }

        public void ButtonClick()
        {
            SoundManager.Instance.PlayClick();
            TooltipManager.Instance.ShowFor(rect, item, OnEditedCallback);
            onSelectCallback?.Invoke(item);
        }

        private void OnEditedCallback(Item item)
        {
            if (item == null)
                return;

            Invalidate(item);

            if (group.ItemType != item.type)
                onEditedCallback?.Invoke(item);
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

            var item = drag.GetComponent<LibraryItem>();
            if (item == null || item.Item == null)
                return;

            var item1 = item.Item;
            var item2 = this.Item;

            if (item1 == item2 || item1.type != item2.type)
                return;

            item.Invalidate(item2);

            this.Invalidate(item1);

            ItemsLibrary.Instance.Swap(item1, item2);

            ResetIconPosition();
        }

        public void Delete()
        {
            if (item == null)
                return;

            ResetIconPosition();

            rootScrollRect.enabled = true;
            canvasGroup.blocksRaycasts = true;
            DragEnded?.Invoke(item);

            ItemsLibrary.Instance.DeleteItem(item);
            group.Refresh();
        }
    }
}