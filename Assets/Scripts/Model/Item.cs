using System;
using System.Collections.Generic;
using UnityEngine;

namespace DnD.Model.Inventory
{
    [System.Serializable]
    public enum EItemType
    {
        Weapon = 0,
        Armor = 1,
        Consumable = 2,
        Instrument = 3,
        Jewerly = 4,
        Quest = 5,
        Misc = 6,
    }

    [System.Serializable]
    public enum EItemRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Mythical = 3,
        Legendary = 4,
        Artifact = 5,
    }

    [System.Serializable]
    public class Item
    {
        public string ID;
        public string name;
        public int icon_group;
        public int icon_id;
        public string description;
        public EItemType type = 0;
        public EItemRarity rarity = 0;
        public int count = 1;
        public float weight; 
        public int costCopper;
        public int costSilver;
        public int costGold;
        public List<string> effects = new();
        public List<ItemProp> props = new();

        public Item Clone()
        {
            var item = new Item();

            item.ID = ID;
            item.name = name;
            item.icon_group = icon_group;
            item.icon_id = icon_id;
            item.description = description;
            item.type = type;
            item.rarity = rarity;
            item.count = count;
            item.weight = weight;
            item.costCopper = costCopper;
            item.costSilver = costSilver;
            item.costGold = costGold;

            for(var i=0;i<effects.Count;i++)
            {
                item.effects.Add(effects[i]);
            }

            for (var i = 0; i < props.Count; i++)
            {
                item.props.Add(props[i].Clone());
            }

            return item;

        }

        public bool IsConsumable => type == EItemType.Consumable;

        public static Item CreateDummy()
        {
            var item = new Item();
            item.ID = Guid.NewGuid().ToString();
            item.count = 1;

            return item;
        }

        public bool HasRarityColor()
        {
            return rarity != EItemRarity.Common;
        }

        public string GetTypeText()
        {
            return this.type switch
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

        public string GetRarityText()
        {
            return rarity switch
            {
                EItemRarity.Uncommon => "Необычная",
                EItemRarity.Rare => "Редкая",
                EItemRarity.Mythical => "Мистическая",
                EItemRarity.Legendary => "Легендарная",
                EItemRarity.Artifact => "Артифакт",
                _ => "Обычная",
            };
        }

        public void SyncModelFrom(Item source)
        {
            if (!source.ID.Equals(ID))
                return;

            name = source.name;
            icon_group = source.icon_group;
            icon_id = source.icon_id;
            description = source.description;
            type = source.type;
            rarity = source.rarity;
            weight = source.weight;
            costCopper = source.costCopper;
            costSilver = source.costSilver;
            costGold = source.costGold;

            effects.Clear();
            for (var i = 0; i < source.effects.Count; i++)
            {
                effects.Add(source.effects[i]);
            }

            props.Clear();
            for (var i = 0; i < source.props.Count; i++)
            {
                props.Add(source.props[i].Clone());
            }
        }

        public Color GetRarityColor()
        {
            return rarity switch
            {
                EItemRarity.Uncommon => new Color(176 / 256f, 195 / 256f, 217 / 256f, 0.5f),
                EItemRarity.Rare => new Color(75 / 256f, 105 / 256f, 255 / 256f, 0.5f),
                EItemRarity.Mythical => new Color(136 / 256f, 71 / 256f, 255 / 256f, 0.5f),
                EItemRarity.Legendary => new Color(211 / 256f, 44 / 256f, 230 / 256f, 0.5f),
                EItemRarity.Artifact => new Color(178 / 256f, 138 / 256f, 51 / 256f, 0.5f),
                _ => new Color(1f, 1f, 1f, 0f),
            };
        }
    }

    [System.Serializable]
    public class ItemProp
    {
        public string name;
        public string value;

        public ItemProp Clone()
        {
            var item = new ItemProp();
            item.name = name;
            item.value = value;
            return item;
        }
    }
}