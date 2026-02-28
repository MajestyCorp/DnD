using DnD.Model;
using DnD.Model.Inventory;
using DnD.Profiles;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DnD.UI.Inventory
{
    public class Inventory : MonoBehaviour, IEditable
    {
        [SerializeField]
        private int minRows = 3;
        [SerializeField]
        private int cols = 7;

        [SerializeField]
        private RectTransform rootContent;

        [SerializeField]
        private InventoryItem itemPrefab;

        private CharacterData data;
        private List<InventoryItem> cells = new ();
        private List<InventoryItem> pool = new ();

        public void ButtonCreate()
        {
            ManageItemPopup.PopupCreate(OnItemCreated);
        }

        public void ButtonAdd()
        {
            ItemsLibraryPopup.Popup(OnItemCreated);
        }

        public void Invalidate(CharacterData data)
        {
            this.data = data;

            if (itemPrefab.gameObject.activeSelf)
                itemPrefab.gameObject.SetActive(false);

            HideCells();
            BuildInventory(data.items);
        }

        public void SetEditable(bool value)
        {
        }

        private void HideCells()
        {
            for(var i=0;i<cells.Count; i++)
            {
                cells[i].gameObject.SetActive(false);
            }

            cells.Clear();
        }

        private void BuildInventory(List<Item> items)
        {
            RemoveEmptyItems(items);

            var minRows = GetNeededRows(items);

            var count = minRows * cols;

            for (var i=0;i<count;i++)
            {
                var cell = GetCell(i);
                var item = i < items.Count ? items[i] : null;

                cell.Initialize(item);
                cells.Add(cell);
            }
        }

        private void RemoveEmptyItems(List<Item> items)
        {
            for(var i=items.Count-1;i>=0;i--)
            {
                var item = items[i];

                if (item != null)
                    return;

                items.RemoveAt(i);
            }
        }

        private InventoryItem GetCell(int index)
        {
            if (pool.Count > index)
                return pool[index];

            var item = Instantiate(itemPrefab, transform);
            pool.Add(item);
            return item;
        }

        private int GetNeededRows(List<Item> items)
        {
            var lastFilledIndex = GetLastFilledItemIndex(items);
            var emptyCells = GetEmptyItems(items);
            var extraLine = emptyCells < cols ? 1 : 0;
            var minRows = Mathf.Max(Mathf.CeilToInt(lastFilledIndex / 7f) + extraLine, this.minRows);

            return minRows;
        }

        private int GetLastFilledItemIndex(List<Item> items)
        {
            var maxIndex = 0;

            for (var i = 0; i < items.Count; i++)
            {
                if (items[i] != null)
                    maxIndex = i;
            }

            return maxIndex;
        }

        private int GetEmptyItems(List<Item> items)
        {
            var count = 0;

            for(var i=0;i<items.Count;i++)
            {
                if (items[i] == null)
                    count++;
            }

            return count;
        }
        public void Save()
        {
            data.items.Clear();

            for(var i=0;i<cells.Count;i++)
            {
                data.items.Add(cells[i].Item);
            }

            data.Save();
        }

        private void OnItemCreated(Item item)
        {
            if (item == null)
                return;

            bool added = false;

            for(var i=0;i<data.items.Count;i++)
            {
                var dataItem = data.items[i];

                if (dataItem != null)
                    continue;

                data.items[i] = item;
                added = true;
                break;
            }

            if (!added)
                data.items.Add(item);

            data.Save();

            Invalidate(data);

            ItemsLibrary.Instance.AddItem(item.Clone());

            StartCoroutine(RebuildLayout());
        }

        private System.Collections.IEnumerator RebuildLayout()
        {
            yield return null;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rootContent);
        }
    }
}