using DnD.Model.Inventory;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace DnD.Model
{
    [Serializable]
    public class CharacterData
    {
        private static string FolderPath => Path.Combine(Application.persistentDataPath, "characters");

        // D&D 5e (2014), Character Advancement: cumulative XP for levels 1 through 20.
        private static readonly int[] levelExperienceThresholds =
        {
            0, 300, 900, 2700, 6500, 14000, 23000, 34000, 48000, 64000,
            85000, 100000, 120000, 140000, 165000, 195000, 225000, 265000, 305000, 355000
        };

        public int dataVersion = 1;

        public bool isSelected = false;
        public string id;              // Уникальный ID персонажа (GUID)
        public string name;            // Имя
        public string race;            // Display name; raceId identifies the catalog entry.
        public string characterClass;  // Display name; characterClassId identifies the catalog entry.
        public string raceId;
        public string characterClassId;
        public string characterSubclassId;
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
        public int armorClassInc = 0;
        public int initiative = 0;
        public int initiativeInc = 0;
        public int level = 1;
        public int experience = 0;
        public int speed = 30;

        public int masteryBonus = 0;

        public ThrowAttributes throws = new();
        public SkillAttributes skills = new();

        public int lifeRolls = 0;
        public int deathRolls = 0;

        public List<Item> items = new();
        public List<TabData> tabs = new();
        public List<string> spellIds = new();

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

        public string GetInitiativeInfo()
        {
            return initiative.ToString() + '+' + initiativeInc.ToString();
        }

        public string GetArmorClassInfo()
        {
            return armorClass.ToString() + '+' + armorClassInc.ToString();
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            // Old files do not contain a version field.
            dataVersion = 0;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            ValidateAndNormalize();
        }

        public void ValidateAndNormalize()
        {
            if (!Guid.TryParse(id, out _))
                throw new JsonSerializationException("Invalid character ID.");
            if (dataVersion > 1)
                throw new JsonSerializationException("Unsupported character version.");

            strength ??= new AttributeValue();
            dexterity ??= new AttributeValue();
            constitution ??= new AttributeValue();
            intelligence ??= new AttributeValue();
            wisdom ??= new AttributeValue();
            charisma ??= new AttributeValue();
            hitPoints ??= new StatValue();
            throws ??= new ThrowAttributes();
            throws.strength ??= new ThrowAttribute();
            throws.dexterity ??= new ThrowAttribute();
            throws.constitution ??= new ThrowAttribute();
            throws.intelligence ??= new ThrowAttribute();
            throws.wisdom ??= new ThrowAttribute();
            throws.charisma ??= new ThrowAttribute();
            skills ??= new SkillAttributes();
            skills.Normalize();
            items ??= new List<Item>();
            tabs ??= new List<TabData>();
            spellIds ??= new List<string>();
            var uniqueSpells = new HashSet<string>(StringComparer.Ordinal);
            spellIds.RemoveAll(id => string.IsNullOrWhiteSpace(id) || !uniqueSpells.Add(id));
            foreach (var item in items)
                item?.Normalize();
            foreach (var tab in tabs)
            {
                if (tab == null)
                    throw new JsonSerializationException("Tab cannot be null.");
                tab.items ??= new List<TabContent>();
                foreach (var content in tab.items)
                {
                    if (content == null)
                        throw new JsonSerializationException("Tab content cannot be null.");
                    if (content is SpellCellsContent slots)
                    {
                        slots.level = Mathf.Max(0, slots.level);
                        slots.maxSlots = Mathf.Max(0, slots.maxSlots);
                        slots.usedSlots = Mathf.Clamp(slots.usedSlots, 0, slots.maxSlots);
                    }
                }
            }

            if (dataVersion == 0)
            {
                // Preserve the old skill totals: the old calculation included save-specific bonuses.
                skills.MigrateLegacyModifiers(this);
                dataVersion = 1;
            }
            Recalculate();
        }

        public static int GetLevelForExperience(int experience)
        {
            for (var index = levelExperienceThresholds.Length - 1; index > 0; index--)
                if (experience >= levelExperienceThresholds[index])
                    return index + 1;
            return 1;
        }

        public void Recalculate()
        {
            experience = Mathf.Max(0, experience);
            level = GetLevelForExperience(experience);

            foreach (var attribute in new[] { strength, dexterity, constitution, intelligence, wisdom, charisma })
                if (attribute.auto)
                    attribute.CalculateModifier();
            throws.Invalidate(this);
            skills.Invalidate(this);
        }

        public bool Save()
        {
            try
            {
                ValidateAndNormalize();
                return JsonStorage.Write(Path.Combine(FolderPath, $"hero_{id}.json"), this);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                JsonStorage.Notify("Не удалось сохранить персонажа. Проверьте его данные и попробуйте ещё раз.");
                return false;
            }
        }

        public static CharacterData Load(string id)
        {
            if (!Guid.TryParse(id, out _))
                return null;
            return LoadPath(Path.Combine(FolderPath, $"hero_{id}.json"));
        }

        private static CharacterData LoadPath(string path)
        {
            return JsonStorage.Read(path, json =>
            {
                var character = JsonStorage.Deserialize<CharacterData>(json);
                if (Path.GetFileName(path) != $"hero_{character.id}.json")
                    throw new JsonSerializationException("Character ID does not match its file.");
                return character;
            });
        }

        public static List<CharacterData> LoadAll()
        {
            var list = new List<CharacterData>();
            if (!Directory.Exists(FolderPath))
                return list;

            try
            {
                var files = new SortedSet<string>(Directory.GetFiles(FolderPath, "hero_*.json"));
                foreach (var backup in Directory.GetFiles(FolderPath, "hero_*.json.bak"))
                    files.Add(backup.Substring(0, backup.Length - 4));
                foreach (var file in files)
                {
                    var data = LoadPath(file);
                    if (data != null)
                        list.Add(data);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                JsonStorage.Notify("Не удалось прочитать папку персонажей. Попробуйте открыть приложение ещё раз.");
            }
            return list;
        }

        public void SyncSameItemsWith(Item source)
        {
            var synced = false;

            for(var i=0;i<items.Count;i++)
            {
                var item = items[i];

                if (item == null || !string.Equals(item.ID, source.ID, StringComparison.Ordinal))
                    continue;

                item.SyncModelFrom(source);
                synced = true;
            }

            if (synced)
                Save();
        }

        public bool Delete()
        {
            return JsonStorage.Delete(Path.Combine(FolderPath, $"hero_{id}.json"));
        }
    }
}
