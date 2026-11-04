using DnD.Model;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DnD.UI.Tabs
{
    public class ManageTabsPopup : Frame, IInitializer
    {
        public static ManageTabsPopup Instance { get; private set; }

        [SerializeField, Header("UI")]
        private Transform content;
        [SerializeField]
        private ManageTabItem itemPrefab;

        private Action<List<TabData>> callback;

        public static void Popup(Action<List<TabData>> onTabsSaved)
        {
            Instance.callback = onTabsSaved;
            Instance.ButtonShow();
        }

        public void InitializeSelf()
        {
            Instance = this;
            itemPrefab.gameObject.SetActive(false);
        }

        public void InitializeAfter()
        {
        }

        public void ButtonSave()
        {
            var tabs = GetCurrentTabs();

            callback?.Invoke(tabs);

            ButtonHide();
        }

        private List<TabData> GetCurrentTabs()
        {
            var items = content.GetComponentsInChildren<ManageTabItem>();
            var result = new List<TabData>();

            if (items!=null && items.Length > 0)
            {
                for(var i=0;i<items.Length; i++)
                {
                    var item = items[i];
                    result.Add(item.Data);
                }
            }

            return result;
        }

        protected override void OnShowAction()
        {
            base.OnShowAction();

            var character = CharacterManager.Instance.ActiveCharacter;

            ClearItems();
            BuildItems(character.tabs);
        }

        private void ClearItems()
        {
            var items = content.GetComponentsInChildren<ManageTabItem>();

            if (items != null && items.Length > 0)
            {
                for (var i = 0; i < items.Length; i++)
                {
                    Destroy(items[i].gameObject);
                }
            }
        }

        private void BuildItems(List<TabData> tabs)
        {
            var list = new List<ManageTabItem>();

            for(var i=0;i<tabs.Count;i++)
            {
                var tab = tabs[i];
                var item = Instantiate(itemPrefab, content);

                item.Initialize(tab);
                list.Add(item);
            }

            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                item.transform.SetSiblingIndex(item.Data.index - CharacterTabGroup.TAB_INDEX);
            }
        }
    }
}