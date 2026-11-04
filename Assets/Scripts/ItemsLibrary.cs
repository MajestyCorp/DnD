using DnD.Model;
using DnD.Model.Inventory;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DnD
{
    public class ItemsLibrary : MonoBehaviour, IInitializer
    {
        public static ItemsLibrary Instance { get; private set; }
        public delegate void ItemHandler(Item item);
        public event ItemHandler OnItemUpdated;
        public event ItemHandler OnItemDeleted;

        private readonly Dictionary<string, Item> items = new();
        private readonly List<Item> list = new();
        private bool dirty;
        private static string FilePath => Path.Combine(Application.persistentDataPath, "items.json");

        public IReadOnlyList<Item> Items => list;

        public void InitializeSelf()
        {
            Instance = this;
            Load();
        }

        public void InitializeAfter() { }

        public bool IsTemplate(Item item)
        {
            return item != null && items.TryGetValue(item.ID, out var template)
                && ReferenceEquals(item, template);
        }

        public void Swap(Item item1, Item item2)
        {
            var index1 = list.IndexOf(item1);
            var index2 = list.IndexOf(item2);
            if (index1 < 0 || index2 < 0)
                return;
            list[index1] = item2;
            list[index2] = item1;
            SaveChanges();
        }

        public void Sync(CharacterData data)
        {
            var changed = false;
            foreach (var otherItem in data.items)
            {
                if (otherItem == null || items.ContainsKey(otherItem.ID))
                    continue;
                var template = otherItem.Clone();
                items.Add(template.ID, template);
                list.Add(template);
                changed = true;
            }
            if (changed)
                SaveChanges();
        }

        public bool AddItem(Item item)
        {
            if (item == null || items.ContainsKey(item.ID))
                return false;
            item.Normalize();
            item.count = 1; // Templates never carry inventory quantities.
            items.Add(item.ID, item);
            list.Add(item);
            SaveChanges();
            return true;
        }

        private void Load()
        {
            var loaded = JsonStorage.Read(FilePath, json =>
            {
                var result = JsonStorage.Deserialize<List<Item>>(json);
                foreach (var item in result)
                {
                    if (item == null)
                        throw new Newtonsoft.Json.JsonSerializationException("Library item cannot be null.");
                    item.Normalize();
                }
                return result;
            });
            if (loaded == null)
                return;
            foreach (var item in loaded)
            {
                if (items.ContainsKey(item.ID))
                    continue;
                item.count = 1;
                items.Add(item.ID, item);
                list.Add(item);
            }
        }

        private void SaveChanges()
        {
            dirty = true;
            Save();
        }

        private void Save()
        {
            if (dirty && JsonStorage.Write(FilePath, list))
                dirty = false;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) Save();
        }

        private void OnApplicationQuit() { Save(); }

        public void DeleteItem(Item itemToDelete)
        {
            if (!items.TryGetValue(itemToDelete.ID, out var item))
                return;
            items.Remove(itemToDelete.ID);
            list.Remove(item);
            SaveChanges();
            OnItemDeleted?.Invoke(itemToDelete);
        }

        public void UpdateModel(Item item)
        {
            // Snapshot before updating any object: source may be the library entry or an inventory item.
            var source = item.Clone();
            if (!items.TryGetValue(item.ID, out var libItem))
            {
                libItem = source.Clone();
                items.Add(libItem.ID, libItem);
                list.Add(libItem);
            }
            else
                libItem.SyncModelFrom(source);
            libItem.count = 1;
            SaveChanges();

            foreach (var character in CharacterManager.Instance.GetCharacters())
                character.SyncSameItemsWith(source);

            OnItemUpdated?.Invoke(item);
        }
    }
}
