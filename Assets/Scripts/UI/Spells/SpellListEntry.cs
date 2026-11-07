using System.Collections.Generic;
using System.Linq;
using DnD.Model;
using DnD.Model.Spells;

namespace DnD.UI.Spells
{
    public class SpellListEntry
    {
        public string Id;
        public SpellDefinition Spell;
        public bool Selected;
        public bool UseStarSelection;
        public string Header;

        public static List<SpellListEntry> Group(IEnumerable<SpellListEntry> entries)
        {
            var result = new List<SpellListEntry>();
            foreach (var group in entries.OrderBy(entry => entry.Spell?.level ?? 10)
                .ThenBy(entry => entry.Spell?.GetText(SpellLibrary.Language).name, System.StringComparer.OrdinalIgnoreCase)
                .GroupBy(entry => entry.Spell?.level ?? 10))
            {
                result.Add(new SpellListEntry { Header = group.Key == 0 ? "Заговоры" : group.Key == 10 ? "Недоступные заклинания" : $"{group.Key} круг" });
                result.AddRange(group);
            }
            return result;
        }

        public static List<SpellListEntry> ForCharacter(CharacterData character, SpellCatalog catalog)
        {
            return character.spellIds.Select(id => new SpellListEntry { Id = id, Spell = catalog.Find(id), Selected = true }).ToList();
        }
    }
}
