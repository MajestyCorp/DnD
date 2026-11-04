using DnD.Model;
using DnD.Model.Inventory;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DnD.UI.Tabs
{
    public class CharacterTabContent : MonoBehaviour
    {
        [SerializeField]
        private Transform addNewItemsText;
        [SerializeField]
        private Transform toolbar;

        private TabData tabData;
        private TabItemUI editingItem;

        public void FillContent(TabData tabData)
        {
            this.tabData = tabData;
            var items = tabData.items;

            for(var i=0;i<items.Count;i++)
            {
                var item = items[i];
                var prefab = TabItemLibrary.Instance.GetItemPrefab(item.contentType);

                var obj = Instantiate(prefab, transform);
                obj.Initialize(this, item);
            }
        }

        public void DeleteItem(TabItemUI item)
        {
            tabData.items.Remove(item.Item);
            CharacterManager.Instance.ActiveCharacter.Save();
        }

        public void CloneItem(TabItemUI item)
        {
            var clone = item.Item.Clone();

            var index = tabData.items.IndexOf(item.Item);
            tabData.items.Insert(index + 1, clone);
            CharacterManager.Instance.ActiveCharacter.Save();

            var prefab = TabItemLibrary.Instance.GetItemPrefab(clone.contentType);

            var objIndex = item.OriginalIndex;
            var obj = Instantiate(prefab, transform);
            obj.Initialize(this, clone);
            obj.SetEditable(true);
            obj.transform.SetSiblingIndex(objIndex + 1);
        }

        public void ItemIndexChanged(TabItemUI item, int indexDelta)
        {
            if (indexDelta == 0)
                return;

            var data = item.Item;
            var index = tabData.items.IndexOf(data);

            if (index >= 0)
            {
                tabData.items.Remove(data);

                var newIndex = index - indexDelta;

                tabData.items.Insert(newIndex, data);

                CharacterManager.Instance.ActiveCharacter.Save();
            }
        }

        public void SetEditable(bool value)
        {
            addNewItemsText.gameObject.SetActive(!value && tabData.items.Count == 0);
            toolbar.gameObject.SetActive(value);

            var items = transform.GetComponentsInChildren<TabItemUI>(true);
            if (items != null)
            {
                for(var i=0;i<items.Length;i++)
                {
                    var item = items[i];
                    item.SetEditable(value);
                }
            }
        }

        public void ButtonAdd()
        {
            SelectTabItemPopup.PopupSelect(OnSelectedItem);
        }

        private void OnSelectedItem(EContentType contentType)
        {
            ManageTabItemPopup.PopupCreate(contentType, OnCreatedItem);
        }

        private void OnCreatedItem(Model.TabContent item)
        {
            tabData.items.Add(item);
            CharacterManager.Instance.ActiveCharacter.Save();

            var prefab = TabItemLibrary.Instance.GetItemPrefab(item.contentType);

            var obj = Instantiate(prefab, transform);
            obj.Initialize(this, item);
            obj.SetEditable(true);
        }

        public void EditItem(TabItemUI item)
        {
            editingItem = item;
            ManageTabItemPopup.PopupUpdate(item.Item, OnUpdatedItem);
        }

        private void OnUpdatedItem(Model.TabContent item)
        {
            CharacterManager.Instance.ActiveCharacter.Save();

            editingItem.Initialize(this, item);
            editingItem.SetEditable(true);
        }
    }
}