using System;
using System.Collections.Generic;
using System.Linq;

namespace DnD.Model.Spells
{
    // Selection belongs to the whole library, never to a filtered list or a pooled UI row.
    public class SpellSelectionDraft
    {
        private readonly SpellCatalog catalog;
        private readonly List<string> ordered = new();
        private readonly HashSet<string> selected = new(StringComparer.Ordinal);
        public CharacterData Character { get; }
        public bool IsActive { get; private set; } = true;
        public int Count => selected.Count;

        public SpellSelectionDraft(CharacterData character, SpellCatalog catalog)
        {
            Character = character ?? throw new ArgumentNullException(nameof(character));
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            foreach (var id in character.spellIds)
                if (!string.IsNullOrWhiteSpace(id) && selected.Add(id)) ordered.Add(id);
        }

        public bool Contains(string id) => id != null && selected.Contains(id);

        public bool Toggle(string id)
        {
            if (!IsActive || string.IsNullOrWhiteSpace(id)) return false;
            if (selected.Remove(id)) { ordered.Remove(id); return true; }
            if (catalog.Find(id) == null) return false;
            selected.Add(id);
            ordered.Add(id);
            return true;
        }

        public bool Apply()
        {
            if (!IsActive) return false;
            var previous = Character.spellIds;
            Character.spellIds = ordered.ToList();
            if (!Character.Save()) { Character.spellIds = previous; return false; }
            IsActive = false;
            return true;
        }

        public void Cancel() { IsActive = false; }
    }
}
