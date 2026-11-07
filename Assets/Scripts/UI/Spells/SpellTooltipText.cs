using System.Collections.Generic;
using DnD.Model.Spells;

namespace DnD.UI.Spells
{
    public static class SpellTooltipText
    {
        public static string Format(SpellDefinition spell, string language)
        {
            var text = spell.GetText(language);
            var lines = new List<string>
            {
                text.name,
                (spell.level == 0 ? "Заговор" : $"{spell.level} круг") + " · " + text.school,
                "Время: " + text.castingTime, "Дистанция: " + text.range,
                "Компоненты: " + text.components, "Длительность: " + text.duration
            };
            if (!string.IsNullOrWhiteSpace(text.materials)) lines.Add("Материалы: " + text.materials);
            if (spell.ritual) lines.Add("Ритуал");
            if (spell.concentration) lines.Add("Концентрация");
            lines.Add("");
            lines.Add(text.description);
            return string.Join("\n", lines);
        }
    }
}
