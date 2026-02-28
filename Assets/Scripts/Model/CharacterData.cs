using DnD.Model.Inventory;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace DnD.Model
{
    [Serializable]
    public class CharacterData
    {
        private static string FolderPath => Path.Combine(Application.persistentDataPath, "characters");

        public bool isSelected = false;
        public string id;              // Уникальный ID персонажа (GUID)
        public string name;            // Имя
        public string race;            // Раса
        public string characterClass;  // Класс
        public string alignment;       // Мировоззрение
        public bool isMale;
        public int avatarIndex;

        // Основные характеристики (с модификаторами)
        public AttributeValue strength;
        public AttributeValue dexterity;
        public AttributeValue constitution;
        public AttributeValue intelligence;
        public AttributeValue wisdom;
        public AttributeValue charisma;

        // Характеристики с текущим и максимальным значением
        public StatValue hitPoints;

        // Прочие характеристики (простые значения)
        public int avatarId = -1;
        public int armorClass = 10;
        public int initiative = 0;
        public int level = 1;
        public int experience = 0;
        public int speed = 30;

        public int masteryBonus = 0;

        public ThrowAttributes throws = new();
        public SkillAttributes skills = new();

        public int lifeRolls = 0;
        public int deathRolls = 0;

        public List<Item> items = new();

        public CharacterData()
        {
            id = Guid.NewGuid().ToString();
            strength = new AttributeValue();
            dexterity = new AttributeValue();
            constitution = new AttributeValue();
            intelligence = new AttributeValue();
            wisdom = new AttributeValue();
            charisma = new AttributeValue();
            hitPoints = new StatValue();
            isMale = true;
            avatarIndex = 0;
            items = new();
        }

        public void Save()
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            var json = JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include
            });

            //var json = JsonUtility.ToJson(this, true);
            string path = Path.Combine(FolderPath, $"hero_{this.id}.json");
            File.WriteAllText(path, json);
        }

        public static CharacterData Load(string id)
        {
            string path = Path.Combine(FolderPath, $"hero_{id}.json");
            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<CharacterData>(json);
            //return JsonUtility.FromJson<CharacterData>(json);
        }

        public static List<CharacterData> LoadAll()
        {
            if (!Directory.Exists(FolderPath))
                return new List<CharacterData>();

            var files = Directory.GetFiles(FolderPath, "hero_*.json");
            var list = new List<CharacterData>();
            foreach (var file in files)
            {
                string json = File.ReadAllText(file);
                var data = JsonConvert.DeserializeObject<CharacterData>(json);
                //var data = JsonUtility.FromJson<CharacterData>(json);
                if (data != null)
                    list.Add(data);
            }
            return list;
        }

        public void SyncSameItemsWith(Item source)
        {
            var synced = false;

            for(var i=0;i<items.Count;i++)
            {
                var item = items[i];

                if (item == null || !item.ID.Equals(source.ID))
                    continue;

                item.SyncModelFrom(source);
                synced = true;
            }

            if (synced)
                Save();
        }

        public void Delete()
        {
            string path = Path.Combine(FolderPath, $"hero_{this.id}.json");
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}