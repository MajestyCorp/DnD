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
            attributes = new Dictionary<string, SkillAttribute>
            {
                { Normalize("Акробатика"), new SkillAttribute() },
                { Normalize("Анализ"), new SkillAttribute() },
                { Normalize("Атлетика"), new SkillAttribute() },
                { Normalize("Внимательность"), new SkillAttribute() },
                { Normalize("Выживание"), new SkillAttribute() },
                { Normalize("Выступление"), new SkillAttribute() },
                { Normalize("Запугивание"), new SkillAttribute() },
                { Normalize("История"), new SkillAttribute() },
                { Normalize("Ловкость рук"), new SkillAttribute() },
                { Normalize("Магия"), new SkillAttribute() },
                { Normalize("Медицина"), new SkillAttribute() },
                { Normalize("Обман"), new SkillAttribute() },
                { Normalize("Природа"), new SkillAttribute() },
                { Normalize("Проницательность"), new SkillAttribute() },
                { Normalize("Религия"), new SkillAttribute() },
                { Normalize("Скрытность"), new SkillAttribute() },
                { Normalize("Убеждение"), new SkillAttribute() },
                { Normalize("Уход за животными"), new SkillAttribute() },
            };
        }

        private string Normalize(string s)
        {
            return s.Normalize(NormalizationForm.FormC);
        }

        public void Invalidate(CharacterData data)
        {
            var throws = data.throws;
            attributes[Normalize("Акробатика")].Invalidate(throws.dexterity);
            attributes[Normalize("Анализ")].Invalidate(throws.intelligence);
            attributes[Normalize("Атлетика")].Invalidate(throws.strength);
            attributes[Normalize("Внимательность")].Invalidate(throws.wisdom);
            attributes[Normalize("Выживание")].Invalidate(throws.wisdom);
            attributes[Normalize("Выступление")].Invalidate(throws.charisma);
            attributes[Normalize("Запугивание")].Invalidate(throws.charisma);
            attributes[Normalize("История")].Invalidate(throws.intelligence);
            attributes[Normalize("Ловкость рук")].Invalidate(throws.dexterity);
            attributes[Normalize("Магия")].Invalidate(throws.intelligence);
            attributes[Normalize("Медицина")].Invalidate(throws.wisdom);
            attributes[Normalize("Обман")].Invalidate(throws.charisma);
            attributes[Normalize("Природа")].Invalidate(throws.intelligence);
            attributes[Normalize("Проницательность")].Invalidate(throws.wisdom);
            attributes[Normalize("Религия")].Invalidate(throws.intelligence);
            attributes[Normalize("Скрытность")].Invalidate(throws.dexterity);
            attributes[Normalize("Убеждение")].Invalidate(throws.charisma);
            attributes[Normalize("Уход за животными")].Invalidate(throws.wisdom);
        }
    }
}