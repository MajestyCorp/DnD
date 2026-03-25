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