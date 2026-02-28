using DnD.Profiles;
using DnD.UI.Inventory;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DnD.UI
{
    public class SelectIconPopup : Frame, IInitializer
    {
        public static SelectIconPopup Instance { get; private set; }

        [SerializeField]
        private RectTransform content;
        [SerializeField]
        private ItemGalleryGroup prefab;

        private ItemIconsProfile profile;
        private ItemIconsProfile lastProfile = null;

        private List<ItemGalleryGroup> groups = new();
        private Action<int, int> callbackOnSelect;
        private Action callbackOnCancel;

        public static void Popup(ItemIconsProfile profile, Action<int, int> callbackOnSelect, Action callbackOnCancel)
        {
            Instance.profile = profile;
            Instance.callbackOnSelect = callbackOnSelect;
            Instance.callbackOnCancel = callbackOnCancel;
            Instance.ButtonShow();
        }

        public void InitializeSelf()
        {
            Instance = this;

            prefab.gameObject.SetActive(false);
        }

        public void SelectItem(int group, int id)
        {
            callbackOnSelect?.Invoke(group, id);

            Hide();
        }

        public void InitializeAfter()
        {
        }

        protected override void OnShowAction()
        {
            base.OnShowAction();

            ClearItems();
            BuildItems();
        }

        private void ClearItems()
        {
            for(var i=0;i<groups.Count;i++)
            {
                Destroy(groups[i].gameObject);
            }

            groups.Clear();
        }

        private void BuildItems()
        {
            var items = profile.Groups;

            for (var i = 0; i < items.Count; i++)
            {
                var data = items[i];
                var group = Instantiate(prefab, content);

                group.Initialize(data, i, OnItemSelected);
                groups.Add(group);
            }
        }

        private void OnItemSelected(int group, int id)
        {
            callbackOnSelect?.Invoke(group, id);
            Hide();
        }

        public void Close()
        {
            callbackOnCancel?.Invoke();
            ButtonHide();
        }
    }
}