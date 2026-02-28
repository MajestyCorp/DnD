using UnityEngine;
using System;
using System.Collections.Generic;

namespace DnD.Model
{
    [Serializable]
    public class SkillAttributes
    {
        public Dictionary<string, SkillAttribute> attributes;

        public SkillAttributes()
        {
            attributes = new Dictionary<string, SkillAttribute>
            {
                { "Акробатика", new SkillAttribute() },
                { "Анализ", new SkillAttribute() },
                { "Атлетика", new SkillAttribute() },
                { "Внимательность", new SkillAttribute() },
                { "Выживание", new SkillAttribute() },
                { "Выступление", new SkillAttribute() },
                { "Запугивание", new SkillAttribute() },
                { "История", new SkillAttribute() },
                { "Ловкость рук", new SkillAttribute() },
                { "Магия", new SkillAttribute() },
                { "Медицина", new SkillAttribute() },
                { "Обман", new SkillAttribute() },
                { "Природа", new SkillAttribute() },
                { "Проницательность", new SkillAttribute() },
                { "Религия", new SkillAttribute() },
                { "Скрытность", new SkillAttribute() },
                { "Убеждение", new SkillAttribute() },
                { "Уход за животными", new SkillAttribute() },
            };
        }

        public void Invalidate(CharacterData data)
        {
            var throws = data.throws;
            attributes["Акробатика"].Invalidate(throws.dexterity);
            attributes["Анализ"].Invalidate(throws.intelligence);
            attributes["Атлетика"].Invalidate(throws.strength);
            attributes["Внимательность"].Invalidate(throws.wisdom);
            attributes["Выживание"].Invalidate(throws.wisdom);
            attributes["Выступление"].Invalidate(throws.charisma);
            attributes["Запугивание"].Invalidate(throws.charisma);
            attributes["История"].Invalidate(throws.intelligence);
            attributes["Ловкость рук"].Invalidate(throws.dexterity);
            attributes["Магия"].Invalidate(throws.intelligence);
            attributes["Медицина"].Invalidate(throws.wisdom);
            attributes["Обман"].Invalidate(throws.charisma);
            attributes["Природа"].Invalidate(throws.intelligence);
            attributes["Проницательность"].Invalidate(throws.wisdom);
            attributes["Религия"].Invalidate(throws.intelligence);
            attributes["Скрытность"].Invalidate(throws.dexterity);
            attributes["Убеждение"].Invalidate(throws.charisma);
            attributes["Уход за животными"].Invalidate(throws.wisdom);
        }
    }
}