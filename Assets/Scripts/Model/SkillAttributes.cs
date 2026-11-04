using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace DnD.Model
{
    [Serializable]
    public class SkillAttributes
    {
        public Dictionary<string, SkillAttribute> attributes;

        public SkillAttributes()
        {
            attributes = new Dictionary<string, SkillAttribute>();

            attributes.TryAdd("Акробатика", new SkillAttribute());
            attributes.TryAdd("Анализ", new SkillAttribute());
            attributes.TryAdd("Атлетика", new SkillAttribute());
            attributes.TryAdd("Внимательность", new SkillAttribute());
            attributes.TryAdd("Выживание", new SkillAttribute());
            attributes.TryAdd("Выступление", new SkillAttribute());
            attributes.TryAdd("Запугивание", new SkillAttribute());
            attributes.TryAdd("История", new SkillAttribute());
            attributes.TryAdd("Ловкость рук", new SkillAttribute());
            attributes.TryAdd("Магия", new SkillAttribute());
            attributes.TryAdd("Медицина", new SkillAttribute());
            attributes.TryAdd("Обман", new SkillAttribute());
            attributes.TryAdd("Природа", new SkillAttribute());
            attributes.TryAdd("Проницательность", new SkillAttribute());
            attributes.TryAdd("Религия", new SkillAttribute());
            attributes.TryAdd("Скрытность", new SkillAttribute());
            attributes.TryAdd("Убеждение", new SkillAttribute());
            attributes.TryAdd("Уход за животными", new SkillAttribute());
        }

        public void Normalize()
        {
            var defaults = new SkillAttributes().attributes;
            attributes ??= new Dictionary<string, SkillAttribute>();
            foreach (var key in defaults.Keys)
                if (!attributes.TryGetValue(key, out var value) || value == null)
                    attributes[key] = defaults[key];
        }

        public void MigrateLegacyModifiers(CharacterData data)
        {
            foreach (var pair in attributes)
            {
                var savingThrow = GetRelatedSavingThrow(data, pair.Key);
                if (savingThrow == null)
                    continue;
                pair.Value.modifier += savingThrow.modifier
                    + ((savingThrow.master ? 1 : 0) - (pair.Value.master ? 1 : 0)) * data.masteryBonus;
            }
        }

        public static bool IsRelatedAbilitySelected(CharacterData data, string skill)
        {
            return data != null && GetRelatedSavingThrow(data, skill)?.master == true;
        }

        private static ThrowAttribute GetRelatedSavingThrow(CharacterData data, string skill)
        {
            return skill switch
            {
                "Атлетика" => data.throws.strength,
                "Акробатика" or "Ловкость рук" or "Скрытность" => data.throws.dexterity,
                "Анализ" or "История" or "Магия" or "Природа" or "Религия" => data.throws.intelligence,
                "Внимательность" or "Выживание" or "Медицина" or "Проницательность" or "Уход за животными" => data.throws.wisdom,
                "Выступление" or "Запугивание" or "Обман" or "Убеждение" => data.throws.charisma,
                _ => null
            };
        }

        public void Invalidate(CharacterData data)
        {
            attributes["Акробатика"].Invalidate(data.dexterity, data.masteryBonus);
            attributes["Анализ"].Invalidate(data.intelligence, data.masteryBonus);
            attributes["Атлетика"].Invalidate(data.strength, data.masteryBonus);
            attributes["Внимательность"].Invalidate(data.wisdom, data.masteryBonus);
            attributes["Выживание"].Invalidate(data.wisdom, data.masteryBonus);
            attributes["Выступление"].Invalidate(data.charisma, data.masteryBonus);
            attributes["Запугивание"].Invalidate(data.charisma, data.masteryBonus);
            attributes["История"].Invalidate(data.intelligence, data.masteryBonus);
            attributes["Ловкость рук"].Invalidate(data.dexterity, data.masteryBonus);
            attributes["Магия"].Invalidate(data.intelligence, data.masteryBonus);
            attributes["Медицина"].Invalidate(data.wisdom, data.masteryBonus);
            attributes["Обман"].Invalidate(data.charisma, data.masteryBonus);
            attributes["Природа"].Invalidate(data.intelligence, data.masteryBonus);
            attributes["Проницательность"].Invalidate(data.wisdom, data.masteryBonus);
            attributes["Религия"].Invalidate(data.intelligence, data.masteryBonus);
            attributes["Скрытность"].Invalidate(data.dexterity, data.masteryBonus);
            attributes["Убеждение"].Invalidate(data.charisma, data.masteryBonus);
            attributes["Уход за животными"].Invalidate(data.wisdom, data.masteryBonus);
        }
    }
}