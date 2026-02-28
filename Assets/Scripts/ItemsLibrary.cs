using DnD.Model;
using DnD.Model.Inventory;
using Newtonsoft.Json;
using System;
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

        private Dictionary<string, Item> items = new();
        private List<Item> list = new();

        public IReadOnlyList<Item> Items => list;

        public void InitializeSelf()
        {
            Instance = this;

            Load();
        }

        public void InitializeAfter()
        {
        }

        public void Swap(Item item1, Item item2)
        {
            var index1 = list.IndexOf(item1);
            var index2 = list.IndexOf(item2);

            list[index1] = item2;
            list[index2] = item1;
        }

        public void Sync(CharacterData data)
        {
            var otherItems = data.items;

            for(var i=0;i<otherItems.Count;i++)
            {
                var otherItem = otherItems[i];

                if (otherItem == null || items.ContainsKey(otherItem.ID))
                    continue;

                var item = otherItem.Clone();
                items[otherItem.ID] = item;
                list.Add(item);
            }
        }

        public bool AddItem(Item item)
        {
            if (items.ContainsKey(item.ID))
                return false;

            items[item.ID] = item;
            list.Add(item);
            return true;
        }

        private void Load()
        {
            var files = Directory.GetFiles(Application.persistentDataPath, "items.json");

            foreach (var file in files)
            {
                string json = File.ReadAllText(file);
                var list = JsonConvert.DeserializeObject<List<Item>>(json);

                if (list == null)
                    continue;

                for (var i=0;i<list.Count;i++)
                {
                    var item = list[i];

                    if (items.ContainsKey(item.ID))
                        continue;

                    items[item.ID] = item;
                    this.list.Add(item);
                }
            }
        }

        private void Save()
        {
            var json = JsonConvert.SerializeObject(list, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include
            });

            string path = Path.Combine(Application.persistentDataPath, "items.json");
            File.WriteAllText(path, json);
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        public void DeleteItem(Item itemToDelete)
        {
            if (!items.TryGetValue(itemToDelete.ID, out var item))
                return;

            items.Remove(itemToDelete.ID);
            list.Remove(item);

            OnItemDeleted?.Invoke(itemToDelete);
        }

        public void UpdateModel(Item item)
        {
            if (!items.TryGetValue(item.ID, out var libItem))
                return;

            libItem.SyncModelFrom(item);

            var characters = CharacterManager.Instance.GetCharacters();

            for(var i=0;i<characters.Count;i++)
            {
                var character = characters[i];
                character.SyncSameItemsWith(item);
            }

            OnItemUpdated?.Invoke(item);
        }
    }
}