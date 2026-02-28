using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DnD.Profiles;
using System;
using DnD.Model.Inventory;

namespace DnD.UI.Inventory
{
    public class LibraryGroup : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text title;
        [SerializeField]
        private LibraryItem prefab;
        [SerializeField]
        private Transform content;

        private Action<Item> onSelectCallback;
        private Action<Item> onEditedCallback;
        private EItemType itemType;
        private List<LibraryItem> list = new();

        public EItemType ItemType => itemType;

        public void Initialize(EItemType itemType, IReadOnlyList<Item> items, Action<Item> onSelectCallback, Action<Item> onEditedCallback)
        {
            if (prefab.gameObject.activeSelf)
            {
                prefab.gameObject.SetActive(false);
            }

            name = title.text = GetName(itemType);
            this.itemType = itemType;
            this.onSelectCallback = onSelectCallback;
            this.onEditedCallback = onEditedCallback;

            ClearItems();
            Build(itemType, items);

            gameObject.SetActive(list.Count > 0);
        }

        private string GetName(EItemType itemType)
        {
            return itemType switch
            {
                EItemType.Armor => "Броня",
                EItemType.Consumable => "Расходные материалы",
                EItemType.Instrument => "Инструменты",
                EItemType.Jewerly => "Украшения",
                EItemType.Quest => "Предметы квестов",
                EItemType.Misc => "Разное",
                _ => "Оружие",
            };
        }

        private void ClearItems()
        {
            for(var i=0;i<list.Count;i++)
            {
                Destroy(list[i].gameObject);
            }

            list.Clear();
        }

        private void Build(EItemType itemType, IReadOnlyList<Item> items)
        {
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];

                if (item.type != itemType)
                    continue;

                var libraryItem = Instantiate(prefab, content);

                libraryItem.Initialize(this, item, onSelectCallback, onEditedCallback);
                list.Add(libraryItem);
            }
        }

        public void Refresh()
        {
            var items = ItemsLibrary.Instance.Items;

            ClearItems();
            Build(itemType, items);

            gameObject.SetActive(list.Count > 0);
        }
    }
}