using DnD.Model;
using DnD.Model.Spells;

namespace DnD
{
    public static class CharacterSpellSelection
    {
        public static bool Add(CharacterData character, SpellCatalog catalog, string spellId)
        {
            if (character == null || catalog?.Find(spellId) == null || character.spellIds.Contains(spellId))
                return false;
            character.spellIds.Add(spellId);
            if (character.Save()) return true;
            character.spellIds.Remove(spellId);
            return false;
        }

        public static bool Move(CharacterData character, string spellId, int targetIndex)
        {
            if (character == null) return false;
            var sourceIndex = character.spellIds.IndexOf(spellId);
            if (sourceIndex < 0 || targetIndex < 0 || targetIndex >= character.spellIds.Count) return false;
            if (sourceIndex == targetIndex) return true;
            var previous = character.spellIds;
            var reordered = new System.Collections.Generic.List<string>(previous);
            reordered.RemoveAt(sourceIndex);
            reordered.Insert(targetIndex, spellId);
            character.spellIds = reordered;
            if (character.Save()) return true;
            character.spellIds = previous;
            return false;
        }

        public static bool Remove(CharacterData character, string spellId)
        {
            if (character == null) return false;
            var index = character.spellIds.IndexOf(spellId);
            if (index < 0) return false;
            character.spellIds.RemoveAt(index);
            if (character.Save()) return true;
            character.spellIds.Insert(System.Math.Min(index, character.spellIds.Count), spellId);
            return false;
        }
    }
}
