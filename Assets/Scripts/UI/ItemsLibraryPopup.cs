using DnD.Model.Inventory;
using DnD.UI.Inventory;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DnD.UI
{
    public class ItemsLibraryPopup : Frame, IInitializer
    {
        public static ItemsLibraryPopup Instance { get; private set; }

        [SerializeField]
        private RectTransform content;
        [SerializeField]
        private LibraryGroup groupPrefab;

        private Action<Item> callback;
        private List<LibraryGroup> groups = new();
        private Item activeItem = null;

        public static void Popup(Action<Item> callback)
        {
            Instance.callback = callback;
            Instance.ButtonShow();
        }

        public void InitializeSelf()
        {
            Instance = this;

            if (groupPrefab.gameObject.activeSelf)
                groupPrefab.gameObject.SetActive(false);
        }

        public void InitializeAfter()
        {
        }

        protected override void OnShowAction()
        {
            base.OnShowAction();

            activeItem = null;

            ClearItems();
            BuildItems();

            ItemsLibrary.Instance.OnItemDeleted += OnItemDeleted;
        }

        protected override void OnHideAction()
        {
            base.OnHideAction();

            ItemsLibrary.Instance.OnItemDeleted -= OnItemDeleted;
        }

        private void OnItemDeleted(Item item)
        {
            if (activeItem == null || !activeItem.ID.Equals(item.ID))
                return;

            activeItem = null;
        }

        private void ClearItems()
        {
            for (var i = 0; i < groups.Count; i++)
            {
                Destroy(groups[i].gameObject);
            }

            groups.Clear();
        }

        private void BuildItems()
        {
            var items = ItemsLibrary.Instance.Items;

            for (var i = EItemType.Weapon; i <= EItemType.Misc; i++)
            {
                var group = Instantiate(groupPrefab, content);
                group.Initialize(i, items, OnSelectCallback, OnEditedCallback);
                groups.Add(group);
            }
        }

        private void OnSelectCallback(Item item)
        {
            activeItem = item;
        }

        private void OnEditedCallback(Item item)
        {
            activeItem = null;

            ClearItems();
            BuildItems();
        }

        public void ButtonSelect()
        {
            if (activeItem == null)
            {
                SoundManager.Instance.PlayNegative();
                return;
            }

            SoundManager.Instance.PlayClick();
            callback?.Invoke(activeItem.Clone());
            Hide();
        }

        public void ButtonCreate()
        {
            ManageItemPopup.PopupCreate(OnCreatedCallback);
        }

        private void OnCreatedCallback(Item item)
        {
            if (item == null)
                return;

            if (!ItemsLibrary.Instance.AddItem(item))
                return;

            activeItem = null;

            for (var i=0;i<groups.Count;i++)
            {
                var group = groups[i];

                if (group.ItemType != item.type)
                    continue;

                group.Initialize(group.ItemType, ItemsLibrary.Instance.Items, OnSelectCallback, OnEditedCallback);
                break;
            }
        }

        public void Close()
        {
            callback?.Invoke(null);
            ButtonHide();
        }
    }
}